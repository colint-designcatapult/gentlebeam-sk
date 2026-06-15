; Include this file after all other include-file:

!ifndef SETUP_SECTIONS_NSH
!define SETUP_SECTIONS_NSH

; Programs
!include "include\Sections\Programs\SEC_HERACLES_INDOOR.nsh"
!include "include\Sections\Programs\SEC_HERACLES_OUTDOOR.nsh"

; Shortcuts
!include "include\Sections\Shortcuts\SEC_DESKTOP_SHORTCUTS.nsh"
!include "include\Sections\Shortcuts\SEC_START_MENU_SHORTCUTS.nsh"

; Common
!include "include\Sections\SEC_BackupExistsInstalled.nsh"
!include "include\Sections\SEC_CopyEula.nsh"
!include "include\Sections\SEC_CreateUninstaller.nsh"

!endif