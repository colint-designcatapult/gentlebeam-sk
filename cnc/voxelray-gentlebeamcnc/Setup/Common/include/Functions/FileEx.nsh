!ifndef SETUP_FUNCTIONS_FILE_EX_NSH
!define SETUP_FUNCTIONS_FILE_EX_NSH

!include "Log.nsh"

;--------------------------------
; Flag /x for ignore files and dirs in cmd FILE with flag /r
!define FILE_VS_IGNORE "/x x86 /x x64 /x bin /x obj /x *.user /x .vs"

#####################################################################
# Extract file to OutDir with checking errors and Abort if failed
#####################################################################
!macro FileEx fileCmd
    !insertmacro LogToFile 'File cmd: ${fileCmd}'
    ClearErrors
    File ${fileCmd}
    ${If} ${Errors}
        !insertmacro LogToFile 'File cmd - Failed: ${fileCmd}'
        SetErrorLevel 2
        Abort
    ${EndIf}
!macroend

!endif