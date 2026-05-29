!ifndef SETUP_FUNCTIONS_UNZIP_ARCHIVE_NSH
!define SETUP_FUNCTIONS_UNZIP_ARCHIVE_NSH

!include "Log.nsh"

#####################################################################
# Unzip archive
#####################################################################
!macro UnzipArchive zipFilename outDir
    CreateDirectory '${outDir}'
    !insertmacro Log 'Unzip ${zipFilename} to folder: ${outDir}'
    nsisunz::UnzipToLog ${zipFilename} ${outDir}
    ; Always check result on stack
    Pop $0
    ${If} $0 == "success"
        !insertmacro Log "Unzip completed"
    ${Else}
        !insertmacro Log "Unzip error: $0" ;print error message to log
    ${EndIf}
!macroend

!endif