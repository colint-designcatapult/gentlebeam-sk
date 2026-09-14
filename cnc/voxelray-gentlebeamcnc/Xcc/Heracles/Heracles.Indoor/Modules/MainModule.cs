using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Common;
using Heracles.Application.Helpers;
using Heracles.Application.Helpers.DummyData;
using Heracles.Application.Models;
using Heracles.Application.Models.Settings;
using Heracles.Application.Models.Supervision;
using Heracles.Application.UI.Views;
using Heracles.Core.Models;
using Heracles.Indoor.ViewModels;
using Heracles.Indoor.Views;
using Heracles.Indoor.Views.Dialogs;
using Heracles.Indoor.Views.Patients.Patient.Treatments;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.UserSessions;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI;
using Xcc.Application.ViewModels;
using Xcc.Application.Views;
using Xcc.Application.Views.Approval;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions;
using Xcc.Shared.Views;


namespace Heracles.Indoor.Modules;

internal class MainModule(IRegionManager regionManager, IDialogService dialogService, IExitingModel exitingModel, IHeraclesMainSettings heraclesMainSettings): IModule
{
    public async void OnInitialized(IContainerProvider containerProvider)
    {
        try
        {
            SetupEmissionPower();

            containerProvider.GetContainer().AddExtension(new Diagnostic()); // todo: just for debug

            // Setup user session events subscriptions before we start logging-in
            SetupSessionExpiration(containerProvider);

            // Initialize core database (roles) before login
            await InitializeCoreDatabase(containerProvider);

            // Show first-run setup if no Administrator exists
            if (!await ShowFirstRunSetupIfNeededAsync(containerProvider))
            {
                exitingModel.ExitApplication();
                return;
            }

            await LoginUserAsync(containerProvider);

            if (heraclesMainSettings.DebugPopulateEmptyDBWithDummyData)
            {
                PopulateDatabaseWithDummyData(containerProvider);
            }

            await CheckDeviceSerialIdAsync(containerProvider);

            StartTelemetryService(containerProvider);

            // Prefetch applicator data to have CollimatorModel ready for use
            await FetchCollimatorDataAsync(containerProvider);

            containerProvider.GetContainer().Resolve<CollimatorWatchdog>();

            regionManager.RequestNavigate(Regions.MainRegion, nameof(MainTabsView));
            regionManager.RequestNavigate(Regions.Main.Settings.UserManagementRegion, nameof(UserManagementView));
            regionManager.RequestNavigate(Regions.Main.Settings.UserPermissionsRegion, nameof(UserRolesView));
        }
        catch (Exception ex)
        {
            dialogService.ReportError($"Failed to initialize application.", ex.Message, _ => exitingModel.ExitApplication());
        }
    }

    private static void SetupSessionExpiration(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<SessionExpirationWatchdog>();
        var sessionEvents = containerProvider.Resolve<INotifyUserSessionChanged>();
        var treatmentEventSource = containerProvider.Resolve<LoadForTreatmentEventSource>();
        var planEventSource = containerProvider.Resolve<PlanEventSource>();
        sessionEvents.UserSessionChanged += (_, e) =>
        {
            switch (e.EventType)
            {
                case UserSessionEventType.Open:
                case UserSessionEventType.Unlocked:
                    treatmentEventSource.Start();
                    planEventSource.Start();
                    break;
                default:
                    treatmentEventSource.Stop();
                    planEventSource.Stop();
                    break;
            }
        };
    }

    private void SetupEmissionPower()
    {
        if (heraclesMainSettings.XrayTubePower50kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower50kV = heraclesMainSettings.XrayTubePower50kV;
        }
        if (heraclesMainSettings.XrayTubePower70kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower70kV = heraclesMainSettings.XrayTubePower70kV;
        }
        if (heraclesMainSettings.XrayTubePower100kV is > 0.0 and <= 500.0)
        {
            CurrentCalculator.HvpsPower100kV = heraclesMainSettings.XrayTubePower100kV;
        }
    }

    private void StartTelemetryService(IContainerProvider containerProvider)
    {
        var networkSupervisor = containerProvider.Resolve<NetworkConnectionSupervisor>();

        var telemetryService = containerProvider.Resolve<ITelemetryService>();
        telemetryService.Start();
    }

    private async Task CheckDeviceSerialIdAsync(IContainerProvider containerProvider)
    {
        try
        {
            ISettingsModel settingsModel = containerProvider.Resolve<ISettingsModel>();
            var settings = await settingsModel.FetchSettingsAsync();

            if (string.IsNullOrWhiteSpace(settings.DeviceSerial))
            {
                dialogService.ShowDialog("DeviceSerialView", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        exitingModel.ExitApplication();
                    }
                });
            }
        }
        catch (Exception)
        {
            // TODO: we should log this exception, but we need to start logging after authentication
            dialogService.ReportError(
                StringConstants.SystemSettings.DeviceSerialIdCheckErrorTitle, 
                StringConstants.SystemSettings.DeviceSerialIdCheckError);
        }
    }

    /// <summary>
    /// Initialize core database with roles and permissions.
    /// This runs unconditionally before login. Roles are created only if they don't already exist.
    /// </summary>
    private static async Task InitializeCoreDatabase(IContainerProvider containerProvider)
    {
        try
        {
            var roleCommands = containerProvider.GetContainer().Resolve<IRoleCommands>();
            var permissionCommands = containerProvider.GetContainer().Resolve<IPermissionCommands>();
            var logWriter = containerProvider.GetContainer().Resolve<ILogWriter>();

            // Get existing roles to avoid duplicates
            var existingRoles = await roleCommands.ReadAllAsync();
            var existingRoleNames = existingRoles?.Select(r => r.Name).ToHashSet() ?? new HashSet<string>();

            // Define predefined roles with their permissions
            ICollection<UserRole> predefinedRoles = [
                new UserRole("Administrator") { Permissions = { ClinicalData = true, Treatment = true, SystemCalibration = true, QualityAssurance = true, SystemSettings = true, UserManagement = true, Services = true} },
                new UserRole("RTT") { Permissions = {ClinicalData = true, Treatment = true, QualityAssurance = true} },
                new UserRole("Physicist"){ Permissions = {ClinicalData = true, SystemCalibration = true, QualityAssurance = true} },
                new UserRole("Service"){ Permissions = {QualityAssurance = true, Services = true} },
                new UserRole("Guest") { Permissions = {} }
            ];

            // Create only roles that don't already exist
            foreach (UserRole role in predefinedRoles)
            {
                if (existingRoleNames.Contains(role.Name))
                    continue;

                var storedRole = await roleCommands.CreateAsync(new RoleRecord { Name = role.Name, Description = role.Name });
                
                if (role.Permissions.ClinicalData)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.ClinicalData });
                if (role.Permissions.Treatment)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.Treatment });
                if (role.Permissions.SystemCalibration)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.SystemCalibration });
                if (role.Permissions.QualityAssurance)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.QualityAssurance });
                if (role.Permissions.SystemSettings)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.SystemSettings });
                if (role.Permissions.UserManagement)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.UserManagement });
                if (role.Permissions.Services)
                    await permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.Services });
            }

            await logWriter.LogAsync(
                "Core database initialization complete: roles created or verified",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            var logWriter = containerProvider.GetContainer().Resolve<ILogWriter>();
            await logWriter.LogAsync(
                $"Core database initialization error: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.Error);
            throw;
        }
    }

    /// <summary>
    /// Check if an Administrator user account exists in the database.
    /// </summary>
    private static async Task<bool> AdminAccountExistsAsync(IContainerProvider containerProvider)
    {
        try
        {
            var userRepository = containerProvider.Resolve<IUserRepository>();
            var existingUsers = await userRepository.FetchUsersAsync();

            return existingUsers.Any(user => user.Role?.Name == "Administrator");
        }
        catch
        {
            // If we can't determine, assume it doesn't exist
            return false;
        }
    }

    /// <summary>
    /// Show first-run administrator setup dialog if no Administrator account exists.
    /// </summary>
    private async Task<bool> ShowFirstRunSetupIfNeededAsync(IContainerProvider containerProvider)
    {
        var adminExists = await AdminAccountExistsAsync(containerProvider);
        if (!adminExists)
        {
            var setupCompleted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            dialogService.ShowDialog("FirstRunAdminSetupView", r =>
            {
                setupCompleted.TrySetResult(r.Result == ButtonResult.OK);
            });

            return await setupCompleted.Task;
        }

        return true;
    }

    private static void PopulateDatabaseWithDummyData(IContainerProvider containerProvider)
    {
        var dummySystemData = containerProvider.Resolve<DummySystemData>();
        var dummyEmrData = containerProvider.Resolve<DummyEmrData>();
        System.Threading.Tasks.Task.Run(async () =>
        {
            dummySystemData.PopulateDB();
            await dummyEmrData.PopulateDB();
        }).GetAwaiter().GetResult();
    }

    private static async Task FetchCollimatorDataAsync(IContainerProvider containerProvider)
    {
        var collimatorService = containerProvider.GetContainer().Resolve<CollimatorService>();
        var logWriter = containerProvider.GetContainer().Resolve<ILogWriter>();
        try
        {
            await collimatorService.UpdateCollimatorModelAsync();
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync($"Failed to fetch Collimators: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
        }
    }
    private async Task LoginUserAsync(IContainerProvider containerProvider)
    {
        var userStore = containerProvider.Resolve<IAuthorizedUserStore>();
        var authorizationService = containerProvider.Resolve<IAuthorizationService>();
        var eventAggregator = containerProvider.Resolve<IEventAggregator>();

        if (!string.IsNullOrEmpty(heraclesMainSettings.DebugAuthUsername) && !string.IsNullOrEmpty(heraclesMainSettings.DebugAuthPassword))
        {
            // authorize with debug username & password
            try
            {
                userStore.AuthorizedUser = await authorizationService.LoginAsync(
                    heraclesMainSettings.DebugAuthUsername,
                    heraclesMainSettings.DebugAuthPassword);
            }                
            catch (Exception ex)
            {
                dialogService.ReportError("AutoLogin authorization error", ex.Message);
                exitingModel.ExitApplication();
                throw;
            }
        }
        else
        {
            dialogService.ShowDialog("LoginView", r =>
            {
                if (r.Result == ButtonResult.Cancel)
                {
                    exitingModel.ExitApplication();
                }
            });
        }
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MainTabsView>();
        containerRegistry.RegisterForNavigation<ClinicalDataView>();
        containerRegistry.RegisterForNavigation<ClinicalDataTabsView>();
        containerRegistry.RegisterForNavigation<PlanView>();
        containerRegistry.RegisterForNavigation<TreatmentsView>();
        containerRegistry.RegisterForNavigation<CameraView>();
        containerRegistry.RegisterForNavigation<PatientImagesView>();
        containerRegistry.RegisterForNavigation<ImageViewerView>();

        containerRegistry.RegisterDialog<AcknowledgeSimulationView>();
        containerRegistry.RegisterDialog<AcknowledgePrescriptionView>();

        containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
        containerRegistry.RegisterDialog<FirstRunAdminSetupView, FirstRunAdminSetupViewModel>();
        containerRegistry.RegisterDialog<ApproveView>();
        containerRegistry.RegisterDialog<ApprovalView>();
        containerRegistry.RegisterDialog<DeviceSerialView>();
        containerRegistry.RegisterDialog<InterlocksDialogView>();
        containerRegistry.RegisterDialog<FaultsView>();
        containerRegistry.RegisterDialog<PhotoViewerModalView>();
    }
}