!ifndef SETUP_FUNCTIONS_ENV_VAR_NSH
!define SETUP_FUNCTIONS_ENV_VAR_NSH

!include "Log.nsh"

!macro AddEnvVar varName varValue
    !insertmacro LogToFile 'EnVar::AddValue: variable: "${varName}" value: "${varValue}"'
    EnVar::AddValue '${varName}' '${varValue}'
    Pop $0
    !insertmacro LogToFile 'EnVar::AddValue:  returned=|$0|'
!macroend

!endif