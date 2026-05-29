# Heracles Command & Control: HowTo / Programmer's Guide

## Dependency management
There are gRPC protocol dependencies that require to clone the corresponding repositories and put them to the parent folder of this repository's root:
* ```<xcc_root>/../protos```,
* ```<xcc_root>/../protos_heracles_robotic_arm```.

These repositories should then be checked out to the actual compatible revision.

Currently, we have a custom solution for dependency version tracking, where all the actual branches and revisions are listed in the ```<xcc_root>/Scripts/all_deps_actual_commits.txt``` file, and there's a script ```all_deps_checkout.cmd``` file in the same folder to clone the missing repos and to checkout to the necessary revisions.

On any protos update, ```all_deps_actual_commits.txt``` should be updated with a new revision to have this dependency fixed in the history and easily distributed.

## R&V Docker build hints

If docker container hangs while linking ../utils, try clean the npm cache and reinstall the deps:
~~~~shell
npm unlink ../utils && rm node_modules && npm install
~~~~

## MCC Installation
* Make sure that User DB table contains at least one user.
* Make sure that User Role and Permissions are defined. For an empty DB, it may be done as following:
~~~~sql
INSERT INTO public.role (role_name, description) VALUES ('Administrator', 'Administrator') returning id;
 
INSERT INTO public.user_role(user_id, role_id) VALUES (1, 1);
 
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_USER_MANAGEMENT'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_PATIENTS_TREATMENT'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_PATIENTS_CLINICAL_DATA'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_QUALITY_ASSURANCE'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SERVICES'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SYSTEM_CALIBRATION'::"PERMISSION") returning id;
 
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SYSTEM_SETTINGS'::"PERMISSION") returning id;
~~~~

* Make sure that Head DB table contains at least one head, or add one with the following SQL query:
~~~~sql
INSERT INTO public.head (
id, create_date, type, serial, is_active) VALUES (
'1'::bigint, '2025-03-25 12:11:10.987'::timestamp without time zone, 'TARGETTYPE_6_CELL'::"TARGETTYPE", '123-456'::text, true::boolean)
 returning id;
~~~~

## Custom Shell Launcher

```powershell
function GetUsernameSID($AccountName) {

    $NTUserObject = New-Object System.Security.Principal.NTAccount($AccountName)
    $NTUserSID = $NTUserObject.Translate([System.Security.Principal.SecurityIdentifier])

    return $NTUserSID.Value

}

# Get the SID for the "User" account
$UserSID = GetUsernameSID("User")
$UserSID

# Make sure that the shell launcher is enabled:
Dism /online /Enable-Feature /all /FeatureName:Client-EmbeddedShellLauncher 

# Create a handle to the class instance so we can call the static methods.
#$ShellLauncherClass = [wmiclass]"\\$COMPUTER\${NAMESPACE}:WESL_UserSetting"
$ShellLauncherClass = [wmiclass]"\\localhost\root\standardcimv2\embedded:WESL_UserSetting"

# Define actions to take when the shell program exits.
# 0 - restart shell, 1 - restart device, 2 - shutdown device
$on_exit_action = 0

# Set Heracles app as the custom shell for the user using one of these options:
$ShellLauncherClass.SetCustomShell($UserSID, "C:\GentleBeam\Heracles\Heracles.Indoor\Heracles.Indoor.exe", ($null), ($null), $on_exit_action)
$ShellLauncherClass.SetCustomShell($UserSID, "C:\GentleBeam\Heracles\Heracles.External\Heracles.External.exe", ($null), ($null), $on_exit_action)

# Enable Shell Launcher

$ShellLauncherClass.SetEnabled($TRUE)
```

## HCC Desktop App

### Clean Architecture

### Solution Architecture

### Naming Convention

## Dependency Injection

### Injecting and Usage of Services

### Injecting and Usage of Shared Data

## Data Models

## View Models

### Properties

### Commands

### Callbacks

### DAL APIs

### Camera 
1. Download ffmpeg-n4.4.4-89-g25841e4f90-win64-gpl-shared-4.4.zip
2. Exctract ffmpeg-n4.4.4-89-g25841e4f90-win64-gpl-shared-4.4\bin\* into C:\ffmpeg

 