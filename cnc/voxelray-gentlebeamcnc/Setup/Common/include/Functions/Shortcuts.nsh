!ifndef SETUP_FUNCTIONS_SHORTCUTS_NSH
!define SETUP_FUNCTIONS_SHORTCUTS_NSH

!include "LogicLib.nsh"
!include "Log.nsh"
!include "Timestamp.nsh"

Var /GLOBAL IsSelectedSecStartMenuShortcuts
Var /GLOBAL IsSelectedSecDesktopShortcuts

; Set defaults variables values
!macro ResetToDefaultShortcutVars
    StrCpy $IsSelectedSecStartMenuShortcuts 'true'
    StrCpy $IsSelectedSecDesktopShortcuts 'false'
!macroend

Function CheckSkipStartMenuPage
    ${If} $IsSelectedSecStartMenuShortcuts == "false"
        !insertmacro Log "Start Menu Page skipped"  
        Abort
    ${EndIf}
FunctionEnd

!macro CreateShortcutEx path fileOrDir lnkPath lnkName shortcutArgs 
    SetOutPath      "${path}"
    CreateShortCut  "${lnkPath}\${lnkName}" \
                    "${path}\${fileOrDir}" \
                    "${shortcutArgs}"
!macroend

!macro CreateStartMenuShortcut path fileOrDir lnkName 
    ; This macro sets $StartMenuFolder and skips to MUI_STARTMENU_WRITE_END 
    ; if the "Don't create shortcuts" checkbox is checked...
    ; !insertmacro Log "IsSelectedSecStartMenuShortcuts = $IsSelectedSecStartMenuShortcuts"
    ${If} $IsSelectedSecStartMenuShortcuts == "true"
        CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
        !insertmacro CreateShortcutEx   "${path}"   "${fileOrDir}" \
                    "$SMPROGRAMS\$StartMenuFolder"  "${lnkName}"  \
                    ""
    ${EndIf}
!macroend

!macro CreateDesktopShortcut path fileOrDir lnkName 
    ; Create shrotcuts in Descktop
    ;!insertmacro Log "IsSelectedSecDesktopShortcuts = $IsSelectedSecDesktopShortcuts"
    ${If} $IsSelectedSecDesktopShortcuts == "true"
        !insertmacro CreateShortcutEx   "${path}"   "${fileOrDir}" \
                                        "$DESKTOP"  "${lnkName}" \
                                        ""
    ${EndIf}
!macroend



!endif