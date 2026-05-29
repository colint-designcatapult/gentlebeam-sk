!ifndef SETUP_FUNCTIONS_KILL_PROCESS_NSH
!define SETUP_FUNCTIONS_KILL_PROCESS_NSH

!include "Log.nsh"

#####################################################################
# Kill process and log cmd
#####################################################################
Var /GLOBAL KillProcessCmd
!macro KillProcess processName
    StrCpy $KillProcessCmd 'taskkill /IM "${processName}" /F'    
    !insertmacro LogToFile 'Kill cmd: $KillProcessCmd'        
    nsExec::Exec $KillProcessCmd
!macroend

!endif