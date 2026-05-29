; Required defines before include this file:
; !define LOG_INSTALL_DIR "..."

!ifndef SETUP_FUNCTIONS_LOG_NSH
!define SETUP_FUNCTIONS_LOG_NSH

!include "FileFunc.nsh"

!include "LogicLib.nsh"
!include "Timestamp.nsh"

#####################################################################
# Logs into the log file
#####################################################################
Var /GLOBAL LOG_INSTALL_FILENAME
!macro LogToFile str
    !insertmacro GetInstallDateTime
    
    ; Documents dir for current user
    SetShellVarContext current
    
    ; Create directory if not exists
    ${IfNot} ${FileExists} `${LOG_INSTALL_DIR}\*.*`
        CreateDirectory "${LOG_INSTALL_DIR}"
    ${EndIf}
    
    ; Log filename
    StrCpy $LOG_INSTALL_FILENAME '${LOG_INSTALL_DIR}\$INSTALL_TIMESTAMP_${PRODUCT_NAME}_v${VERSION}_install.log'
    nsislog::log $LOG_INSTALL_FILENAME '$InstallDateTime_NOW: ${str}'
!macroend

#####################################################################
# Logs into the detailed gui section 
#####################################################################
!macro LogToGui str
    ; Log to NSIS Details view
    DetailPrint '${str}'
!macroend

#####################################################################
# Logs into the STDOUT
#####################################################################
!macro LogToStdOut str        
    ; Log to stdout
    System::Call 'kernel32::GetStdHandle(i -11)i.r9'
    System::Call 'kernel32::AttachConsole(i -1)'
    FileWrite $9 '${str}$\r$\n'
!macroend

#####################################################################
# Logs into:
#     - the log file
#     - the detailed gui section 
#     - the STDOUT
#####################################################################
!macro Log str
    !insertmacro LogToFile   '${str}'
    !insertmacro LogToGui    '${str}'
    !insertmacro LogToStdOut '${str}'
!macroend

Function DumpInstallerDetailsLog
    ; Documents dir for current user
    SetShellVarContext current

    ; Create directory if not exists
    ${IfNot} ${FileExists} `${LOG_INSTALL_DIR}\*.*`
        CreateDirectory "${LOG_INSTALL_DIR}"
    ${EndIf}
    
    ; Log filename
    Var /GLOBAL LOG_INSTALL_FILENAME_DETAILS
    StrCpy $LOG_INSTALL_FILENAME_DETAILS '${LOG_INSTALL_DIR}\$INSTALL_TIMESTAMP_${PRODUCT_NAME}_v${VERSION}_install_DetailsView.log'
    
    ; Dump log-text (ShowDetail) to text file
    StrCpy $0 "$LOG_INSTALL_FILENAME_DETAILS"
    Push $0
    Call DumpLog
FunctionEnd

!endif