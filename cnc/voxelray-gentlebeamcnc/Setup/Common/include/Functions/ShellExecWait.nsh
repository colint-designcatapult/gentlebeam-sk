; https://nsis.sourceforge.io/ShellExecWait
; Basic code for waiting on a "ExecShell" (Note that you can only wait on programs, not documents or URL's)
;
; Usage:
;   !insertmacro ShellExecWait "" '"notepad.exe"' '"c:\tmp\Test.txt"' "" ${SW_SHOW} $1
;   MessageBox mb_ok "exitcode or error=$1"
;

!ifndef SETUP_FUNCTIONS_SHELL_EXEC_WAIT_NSH
!define SETUP_FUNCTIONS_SHELL_EXEC_WAIT_NSH

!include LogicLib.nsh
!include WinMessages.nsh
 
!macro ShellExecWait verb app param workdir show exitoutvar ;only app and show must be != "", every thing else is optional
#define SEE_MASK_NOCLOSEPROCESS 0x40 
System::Store S
!if "${NSIS_PTR_SIZE}" > 4
!define /ReDef /math SYSSIZEOF_SHELLEXECUTEINFO 14 * ${NSIS_PTR_SIZE}
!else ifndef SYSSIZEOF_SHELLEXECUTEINFO
!define SYSSIZEOF_SHELLEXECUTEINFO 60
!endif
System::Call '*(&i${SYSSIZEOF_SHELLEXECUTEINFO})i.r0'
System::Call '*$0(i ${SYSSIZEOF_SHELLEXECUTEINFO},i 0x40,p $hwndparent,t "${verb}",t $\'${app}$\',t $\'${param}$\',t "${workdir}",i ${show})p.r0'
System::Call 'shell32::ShellExecuteEx(t)(pr0)i.r1 ?e' ; (t) to trigger A/W selection
${If} $1 <> 0
	System::Call '*$0(is,i,p,p,p,p,p,p,p,p,p,p,p,p,p.r1)' ;stack value not really used, just a fancy pop ;)
	System::Call 'kernel32::WaitForSingleObject(pr1,i-1)'
	System::Call 'kernel32::GetExitCodeProcess(pr1,*i.s)'
	System::Call 'kernel32::CloseHandle(pr1)'
${EndIf}
System::Free $0
!if "${exitoutvar}" == ""
	pop $0
!endif
System::Store L
!if "${exitoutvar}" != ""
	pop ${exitoutvar}
!endif
!macroend

!endif