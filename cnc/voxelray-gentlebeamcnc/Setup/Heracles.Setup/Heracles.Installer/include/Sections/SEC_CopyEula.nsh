!ifndef SETUP_SECTIONS_SEC_COPY_EULA_NSH
!define SETUP_SECTIONS_SEC_COPY_EULA_NSH

LangString DESC_SEC_CopyEula   ${LANG_ENGLISH} "Copy EULA file"

!macro AddSection_SEC_CopyEula
    ; Hidden section from GUI ('-' symbol in section name)
    Section "-Backup" SEC_CopyEula
    ;; Start Section
        SetDetailsView show 
        !insertmacro Log "Start install section: SEC_CopyEula"
            
        SetShellVarContext current

        ; ReadOnly section
        SectionIn RO
        
    ;; Copy EULA
        SetOutPath          "$INSTDIR"
        !insertmacro FileEx "src\EndUserLicenseAgreement.txt"
                
    ;; End Section		
        !insertmacro Log "End install section: SEC_CopyEula"
        ; Dump log-text (ShowDetail) to text file (for current user)
        Call DumpInstallerDetailsLog
    SectionEnd	;; SEC_CopyEula
!macroend

!endif