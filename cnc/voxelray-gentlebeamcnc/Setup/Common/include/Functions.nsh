; Include this file after all other include-file:

!ifndef SETUP_FUNCTIONS_NSH
!define SETUP_FUNCTIONS_NSH

!include "Functions\DumpLog.nsh"
!include "Functions\EnvVar.nsh"
!include "Functions\FileEx.nsh"
!include "Functions\KillProcess.nsh"
!include "Functions\Log.nsh"
!include "Functions\Timestamp.nsh"
!include "Functions\ShellExecWait.nsh"
!include "Functions\Shortcuts.nsh"
!include "Functions\UnzipArchive.nsh"
!include "Functions\DotNetRuntimeCheck.nsh"

!endif