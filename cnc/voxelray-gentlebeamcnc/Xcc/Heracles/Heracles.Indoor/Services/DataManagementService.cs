using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Heracles.Indoor.SqliteGrpcServer;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.Indoor.Services;

/// <summary>Local, session-authorized operations; deliberately not exposed through gRPC.</summary>
public sealed class DataManagementService : IDisposable
{
    private readonly IAuthorizedUserStore _users;
    private readonly IBearerTokenUserSessionManager _sessions;
    private readonly SqlCipherDatabase? _database;
    private readonly SqliteGrpcServerHost? _host;
    private readonly Action _stopEventSources;
    private readonly Func<Task> _shutdownChannel;
    private readonly Func<Func<Task>, Task> _runModalShutdown;
    private readonly Action<string, bool> _reportAndExit;
    private int _busy;
    private bool _disposed;
    private volatile bool _isShuttingDown;

    public DataManagementService(IAuthorizedUserStore users, IBearerTokenUserSessionManager sessions,
        SqlCipherDatabase? database, SqliteGrpcServerHost? host, Action stopEventSources,
        Func<Task> shutdownChannel, Func<Func<Task>, Task> runModalShutdown, Action<string, bool> reportAndExit)
    {
        _users = users;
        _sessions = sessions;
        _database = database;
        _host = host;
        _stopEventSources = stopEventSources;
        _shutdownChannel = shutdownChannel;
        _runModalShutdown = runModalShutdown;
        _reportAndExit = reportAndExit;
        _users.AuthorizedUserChanged += OnAuthorizedUserChanged;
        _sessions.UserSessionChanged += OnSessionChanged;
    }

    public bool IsAvailable => !_disposed && _database is not null && _host is not null;
    public bool CanManage => IsAvailable && !IsShuttingDown &&
        _users.AuthorizedUser is { Id: > 0 } user && !string.IsNullOrWhiteSpace(user.Username) &&
        user.Role?.Name == UserRole.BuiltInNames.Administrator &&
        _sessions.UserSession is { IsLocked: false, IsExpired: false } session &&
        !session.CancellationToken.IsCancellationRequested;
    public bool IsBusy => Volatile.Read(ref _busy) != 0;
    public bool IsShuttingDown => _isShuttingDown;
    public event EventHandler? StateChanged;

    public string RevealRecoveryKey()
    {
        EnsureAuthorized();
        if (IsBusy)
            throw new DataManagementException("Another database operation is already in progress.");
        return _database!.GetRecoveryKey();
    }

    public string RevealDatabasePassword()
    {
        EnsureAuthorized();
        if (IsBusy)
            throw new DataManagementException("Another database operation is already in progress.");
        return _database!.GetDatabasePassword();
    }

    public async Task ExportAsync(string path, string password, CancellationToken cancellationToken = default)
    {
        BeginOperation();
        try
        {
            await _database!.ExportAsync(path, password, cancellationToken,
                CreateTransferAudit("Export database")).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            throw TransferError(exception, isImport: false);
        }
        finally
        {
            password = string.Empty;
            EndOperation();
        }
    }

    public async Task ImportAsync(string path, string password, CancellationToken cancellationToken = default)
    {
        BeginOperation();
        string? staging = null;
        bool installed = false;
        try
        {
            var auditRecord = CreateTransferAudit("Import database configuration and patient records");
            staging = await _database!.PrepareImportAsync(path, password, cancellationToken).ConfigureAwait(false);
            password = string.Empty;
            using var pause = await _host!.PauseUnaryCallsAsync(cancellationToken).ConfigureAwait(false);
            EnsureAuthorized();
            if (await _host.HasActiveTreatmentAsync().ConfigureAwait(false))
                throw new DataManagementException("Import is unavailable while a treatment is pending, partially pending, or loaded. Unload the treatment before importing.");
            cancellationToken.ThrowIfCancellationRequested();

            await _runModalShutdown(async () =>
            {
                // Dispatching the modal surface can take time. This is the last reversible boundary.
                EnsureAuthorized();
                cancellationToken.ThrowIfCancellationRequested();
                _isShuttingDown = true;
                StateChanged?.Invoke(this, EventArgs.Empty);
                _stopEventSources();
                await _host.DisposeAsync().ConfigureAwait(false);
                await _shutdownChannel().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                // Install consumes the staging reservation on success and failure.
                var prepared = staging!;
                staging = null;
                await Task.Run(() => _database.InstallPreparedImport(prepared, auditRecord)).ConfigureAwait(false);
                installed = true;
            }).ConfigureAwait(false);
        }
        catch (Exception) when (IsShuttingDown)
        {
            // No old in-memory model may resume after shutdown has begun, even if replacement failed.
        }
        catch (OperationCanceledException) { throw; }
        catch (DataManagementException) { throw; }
        catch (UnauthorizedAccessException) when (!CanManage) { throw; }
        catch (Exception exception)
        {
            throw TransferError(exception, isImport: true);
        }
        finally
        {
            password = string.Empty;
            try
            {
                if (staging is not null)
                    _database!.DiscardPreparedImport(staging);
            }
            finally
            {
                EndOperation();
                if (IsShuttingDown)
                    _reportAndExit(installed
                        ? "Database imported. The application will close. Start it again to use the imported data."
                        : "Database replacement could not be completed. The application will close. Restart it to open the retained database; if it cannot open, contact your administrator.",
                        !installed);
            }
        }
    }

    private string CreateTransferAudit(string action)
    {
        var user = _users.AuthorizedUser!;
        return JsonSerializer.Serialize(new
        {
            action,
            outcome = "success",
            actor = user.Username,
            userId = user.Id
        });
    }

    private void EnsureAuthorized()
    {
        if (!IsAvailable)
            throw new DataManagementException("Embedded database management is unavailable in this mode.");
        if (!CanManage)
            throw new UnauthorizedAccessException("An administrator with an active, unlocked session is required.");
    }

    private void BeginOperation()
    {
        EnsureAuthorized();
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
            throw new DataManagementException("Another database operation is already in progress.");
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndOperation()
    {
        Interlocked.Exchange(ref _busy, 0);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static DataManagementException TransferError(Exception exception, bool isImport) => new(
        exception switch
        {
            TimeoutException => "Database activity did not finish in time. Import was not started; try again when current operations have finished.",
            IOException or UnauthorizedAccessException => "The selected file or location is unavailable. Check the location, access permissions, and available disk space.",
            _ => isImport
                ? "The database could not be imported. The password may be incorrect, or the database is unreadable, damaged, incompatible, or an unsafe application file."
                : "The database could not be exported. Check the selected location and password. The previous destination has not been replaced."
        });

    private void OnAuthorizedUserChanged(object? sender, IUser? user) => StateChanged?.Invoke(this, EventArgs.Empty);
    private void OnSessionChanged(object? sender, UserSessionEventArgs args) => StateChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _users.AuthorizedUserChanged -= OnAuthorizedUserChanged;
        _sessions.UserSessionChanged -= OnSessionChanged;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>A user-facing error that contains neither provider diagnostics nor secret material.</summary>
public sealed class DataManagementException(string message) : InvalidOperationException(message);
