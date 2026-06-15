!ifndef SETUP_FUNCTIONS_TIMESTAMP_NSH
!define SETUP_FUNCTIONS_TIMESTAMP_NSH

!define /date COMPILE_TIMESTAMP "%Y-%m-%d_%H-%M-%S"

; Initialized in function 'InitInstallerTimestampString'
; Format same as COMPILE_TIMESTAMP: "%Y-%m-%d_%H-%M-%S"
Var /GLOBAL INSTALL_TIMESTAMP

Var /GLOBAL InstallDateTime_NOW

#####################################################################
# Fill variable InstallDateTime_NOW of current installation date/time
#####################################################################
!macro GetInstallDateTime
    ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
    ; $0="01"      day
    ; $1="04"      month
    ; $2="2005"    year
    ; $3="Friday"  day of week name
    ; $4="16"      hour
    ; $5="05"      minute
    ; $6="50"      seconds 
    
    ; Format: "%Y-%m-%d_%H-%M-%S"
    StrCpy $InstallDateTime_NOW '$2-$1-$0_$4-$5-$6'
!macroend

Function InitInstallTimestampString
    ; Format same as COMPILE_TIMESTAMP: "%Y-%m-%d_%H-%M-%S"
    !insertmacro GetInstallDateTime
    StrCpy $INSTALL_TIMESTAMP $InstallDateTime_NOW
FunctionEnd

!endif