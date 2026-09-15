using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Com.Empyreanmed.Heracles.CoilConfigurations.V1;
using Com.Empyreanmed.Heracles.CollimatorConfigurations.V1;
using Com.Empyreanmed.Heracles.Collimators.V1;
using Com.Empyreanmed.Heracles.CorrectionMatrix.V1;
using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Head.V1;
using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Com.Empyreanmed.Heracles.Intensities.V1;
using Com.Empyreanmed.Heracles.Logs.V1;
using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Com.Empyreanmed.Heracles.Patients.V1;
using Com.Empyreanmed.Heracles.Photos.V1;
using Com.Empyreanmed.Heracles.Plans.V1;
using Com.Empyreanmed.Heracles.Positions.V1;
using Com.Empyreanmed.Heracles.PresetConfigurations.V1;
using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Com.Empyreanmed.Heracles.Roles.V1;
using Com.Empyreanmed.Heracles.RolesPermissions.V1;
using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Com.Empyreanmed.Heracles.Settings.V1;
using Com.Empyreanmed.Heracles.Simulations.V1;
using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Com.Empyreanmed.Heracles.Treatments.V1;
using Com.Empyreanmed.Heracles.UserRoles.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Com.Empyreanmed.Heracles.Visits.V1;
using Com.Empyreanmed.Heracles.Warmups.V1;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using Heracles.Indoor.SqliteGrpcServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Heracles.Indoor.SqliteGrpcServer;

/// <summary>
/// Hosts an in-process ASP.NET Core gRPC server backed by SQLite.
/// Call <see cref="StartAsync"/> once at application startup and
/// <see cref="StopAsync"/> on shutdown.
/// </summary>
public sealed class SqliteGrpcServerHost : IAsyncDisposable
{
    public const int DefaultPort = 5199;

    private readonly WebApplication _app;
    private readonly DatabaseMaintenanceGate _maintenance = new();
    private readonly CancellationTokenSource _stopping = new();
    private readonly object _lifecycleLock = new();
    private Task? _stopTask;
    private Task? _disposeTask;

    public SqliteGrpcServerHost(SqlCipherConnectionFactory connections, int port = DefaultPort)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(k =>
        {
            k.ListenAnyIP(port, o => o.Protocols = HttpProtocols.Http2);
        });

        var services = builder.Services;

        var initializeRepositories = new List<Action<IServiceProvider>>();
        void RegisterRepository<T>(string tableName, bool hasParentId = false, string? parentIdJsonField = null)
            where T : class, Google.Protobuf.IMessage<T>, new()
        {
            services.AddSingleton(_ => new SqliteProtoRepository<T>(
                connections, tableName, hasParentId, parentIdJsonField));
            initializeRepositories.Add(provider => provider.GetRequiredService<SqliteProtoRepository<T>>());
        }

        RegisterRepository<Patient>("patients");
        RegisterRepository<Diagnosis>("diagnoses", hasParentId: true);
        RegisterRepository<Simulation>("simulations", hasParentId: true);
        RegisterRepository<Prescription>("prescriptions", hasParentId: true);
        RegisterRepository<Visit>("visits", hasParentId: true);
        RegisterRepository<Plan>("plans", hasParentId: true);
        RegisterRepository<TreatmentDevice>("treatment_devices", hasParentId: true);
        RegisterRepository<Position>("positions", hasParentId: true);
        RegisterRepository<TreatmentField>("treatment_fields", hasParentId: true);
        RegisterRepository<ActualTreatmentField>("actual_treatment_fields", hasParentId: true);
        RegisterRepository<EmissionTreatmentField>("emission_treatment_fields", hasParentId: true);
        RegisterRepository<Treatment>("treatments");
        RegisterRepository<Photo>("photos", hasParentId: true);
        RegisterRepository<Com.Empyreanmed.Heracles.Users.V1.User>("users");
        RegisterRepository<Role>("roles");
        RegisterRepository<RolesPermissions>("roles_permissions");
        RegisterRepository<UserRole>("user_roles");
        RegisterRepository<Head>("heads");
        RegisterRepository<Collimator>("collimators", hasParentId: true);
        RegisterRepository<CollimatorConfiguration>("collimator_configurations");
        RegisterRepository<CoilConfiguration>("coil_configurations");
        RegisterRepository<CorrectionMatrix>("correction_matrices");
        RegisterRepository<HeaterCurrentConfig>("heater_current_configs");
        RegisterRepository<OutputFactor>("output_factors");
        RegisterRepository<ReferenceField>("reference_fields");
        RegisterRepository<PresetConfiguration>("preset_configurations");
        RegisterRepository<QCSample>("qcsamples", hasParentId: true, parentIdJsonField: "collimatorConfigurationId");
        RegisterRepository<QCSampleField>("qcsample_fields", hasParentId: true, parentIdJsonField: "qcsampleId");
        RegisterRepository<Intensity>("intensities", hasParentId: true, parentIdJsonField: "qcsampleFieldsId");
        RegisterRepository<SafetyCheck>("safety_checks");
        RegisterRepository<Warmup>("warmups");
        RegisterRepository<Log>("logs");
        RegisterRepository<Settings>("settings");

        // Service implementations
        services.AddSingleton(connections);
        services.AddSingleton(_maintenance);
        services.AddSingleton<DatabaseMaintenanceInterceptor>();
        services.AddSingleton<AuditSessionRegistry>();
        services.AddGrpc(options =>
        {
            // Drain database operations before maintenance replaces or closes SQLite.
            options.Interceptors.Add<DatabaseMaintenanceInterceptor>();
        });
        services.AddSingleton<AuthServiceImpl>();
        services.AddSingleton<PatientServiceImpl>();
        services.AddSingleton<DiagnosisServiceImpl>();
        services.AddSingleton<SimulationServiceImpl>();
        services.AddSingleton<PrescriptionServiceImpl>();
        services.AddSingleton<VisitServiceImpl>();
        services.AddSingleton<PlanServiceImpl>();
        services.AddSingleton<TreatmentDeviceServiceImpl>();
        services.AddSingleton<PositionServiceImpl>();
        services.AddSingleton<TreatmentFieldServiceImpl>();
        services.AddSingleton<ActualTreatmentFieldServiceImpl>();
        services.AddSingleton<EmissionTreatmentFieldServiceImpl>();
        services.AddSingleton<TreatmentServiceImpl>();
        services.AddSingleton<PhotosServiceImpl>();
        services.AddSingleton<UsersServiceImpl>();
        services.AddSingleton<RoleServiceImpl>();
        services.AddSingleton<RolesPermissionsServiceImpl>();
        services.AddSingleton<UserRoleServiceImpl>();
        services.AddSingleton<HeadServiceImpl>();
        services.AddSingleton<CollimatorServiceImpl>();
        services.AddSingleton<CollimatorConfigurationServiceImpl>();
        services.AddSingleton<CoilConfigurationServiceImpl>();
        services.AddSingleton<CorrectionMatrixServiceImpl>();
        services.AddSingleton<HeaterCurrentConfigServiceImpl>();
        services.AddSingleton<OutputFactorServiceImpl>();
        services.AddSingleton<ReferenceFieldServiceImpl>();
        services.AddSingleton<PresetConfigurationServiceImpl>();
        services.AddSingleton<QCSampleServiceImpl>();
        services.AddSingleton<QCSampleFieldServiceImpl>();
        services.AddSingleton<IntensityServiceImpl>();
        services.AddSingleton<SafetyCheckServiceImpl>();
        services.AddSingleton<WarmupServiceImpl>();
        services.AddSingleton<LogServiceImpl>();
        services.AddSingleton<SettingsServiceImpl>();
        services.AddSingleton<SystemServiceImpl>();

        _app = builder.Build();
        try
        {
            foreach (var initialize in initializeRepositories)
                initialize(_app.Services);
        }
        catch
        {
            _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _stopping.Dispose();
            throw;
        }

        _app.Use(async (context, next) =>
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                context.RequestAborted, _stopping.Token);
            context.RequestAborted = cancellation.Token;
            await next(context);
        });

        _app.MapGrpcService<AuthServiceImpl>();
        _app.MapGrpcService<PatientServiceImpl>();
        _app.MapGrpcService<DiagnosisServiceImpl>();
        _app.MapGrpcService<SimulationServiceImpl>();
        _app.MapGrpcService<PrescriptionServiceImpl>();
        _app.MapGrpcService<VisitServiceImpl>();
        _app.MapGrpcService<PlanServiceImpl>();
        _app.MapGrpcService<TreatmentDeviceServiceImpl>();
        _app.MapGrpcService<PositionServiceImpl>();
        _app.MapGrpcService<TreatmentFieldServiceImpl>();
        _app.MapGrpcService<ActualTreatmentFieldServiceImpl>();
        _app.MapGrpcService<EmissionTreatmentFieldServiceImpl>();
        _app.MapGrpcService<TreatmentServiceImpl>();
        _app.MapGrpcService<PhotosServiceImpl>();
        _app.MapGrpcService<UsersServiceImpl>();
        _app.MapGrpcService<RoleServiceImpl>();
        _app.MapGrpcService<RolesPermissionsServiceImpl>();
        _app.MapGrpcService<UserRoleServiceImpl>();
        _app.MapGrpcService<HeadServiceImpl>();
        _app.MapGrpcService<CollimatorServiceImpl>();
        _app.MapGrpcService<CollimatorConfigurationServiceImpl>();
        _app.MapGrpcService<CoilConfigurationServiceImpl>();
        _app.MapGrpcService<CorrectionMatrixServiceImpl>();
        _app.MapGrpcService<HeaterCurrentConfigServiceImpl>();
        _app.MapGrpcService<OutputFactorServiceImpl>();
        _app.MapGrpcService<ReferenceFieldServiceImpl>();
        _app.MapGrpcService<PresetConfigurationServiceImpl>();
        _app.MapGrpcService<QCSampleServiceImpl>();
        _app.MapGrpcService<QCSampleFieldServiceImpl>();
        _app.MapGrpcService<IntensityServiceImpl>();
        _app.MapGrpcService<SafetyCheckServiceImpl>();
        _app.MapGrpcService<WarmupServiceImpl>();
        _app.MapGrpcService<LogServiceImpl>();
        _app.MapGrpcService<SettingsServiceImpl>();
        _app.MapGrpcService<SystemServiceImpl>();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _app.StartAsync(cancellationToken);

    public Task<IDisposable> PauseUnaryCallsAsync(CancellationToken cancellationToken = default)
        => _maintenance.PauseAsync(cancellationToken);

    public async Task<bool> HasActiveTreatmentAsync()
    {
        var plans = await _app.Services.GetRequiredService<SqliteProtoRepository<Plan>>().ReadAllAsync();
        return plans.Any(plan => plan.TreatmentLoadingState is
            Com.Empyreanmed.Heracles.Enums.V1.TREATMENTLOADINGSTATE.Pendingload or
            Com.Empyreanmed.Heracles.Enums.V1.TREATMENTLOADINGSTATE.Partialpendingload or
            Com.Empyreanmed.Heracles.Enums.V1.TREATMENTLOADINGSTATE.Loaded);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_lifecycleLock)
            return _stopTask ??= StopCoreAsync(cancellationToken);
    }

    private async Task StopCoreAsync(CancellationToken cancellationToken)
    {
        _maintenance.Stop();
        await _stopping.CancelAsync().ConfigureAwait(false);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await _app.StopAsync(timeout.Token).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        lock (_lifecycleLock)
            return new ValueTask(_disposeTask ??= DisposeCoreAsync());
    }

    private async Task DisposeCoreAsync()
    {
        try
        {
            await StopAsync().ConfigureAwait(false);
        }
        finally
        {
            await _app.DisposeAsync().ConfigureAwait(false);
            _stopping.Dispose();
        }
    }
}
