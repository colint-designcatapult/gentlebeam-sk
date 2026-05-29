!ifndef SETUP_SECTIONS_SEC_CREATE_UNINSTALLER_NSH
!define SETUP_SECTIONS_SEC_CREATE_UNINSTALLER_NSH

LangString DESC_SEC_CreateUninstaller   ${LANG_ENGLISH} "Create uninstaller"

!macro AddSection_SEC_CreateUninstaller
    ; Hidden section from GUI ('-' symbol in section name)
    Section "-Create Uninstaller" SEC_CreateUninstaller
    ;; Start Section
        SetDetailsView show 
        !insertmacro Log "Start install section: SEC_CreateUninstaller"
            
        SetShellVarContext current

        ; ReadOnly section
        SectionIn RO
        
    ;; Create uninstaller
        WriteUninstaller "$INSTDIR\Uninstall.exe"   
            
        ;; Create Registry keys for Add/Remove programs of Current user (HKCU)
        WriteRegStr HKCU "${ADD_REMOVE_REGISTRY_KEY}" "DisplayName"     "${PRODUCT_NAME}"
        WriteRegStr HKCU "${ADD_REMOVE_REGISTRY_KEY}" "UninstallString" "$INSTDIR\Uninstall.exe"
        
    ;; Create shortcuts
        !insertmacro MUI_STARTMENU_WRITE_BEGIN "Application"
        !insertmacro CreateStartMenuShortcut "$INSTDIR"		"Uninstall.exe"		"Uninstall.lnk"
        !insertmacro MUI_STARTMENU_WRITE_END
            
            
    ;; End Section		
        !insertmacro Log "End install section: SEC_CreateUninstaller"
        ; Dump log-text (ShowDetail) to text file (for current user)
        Call DumpInstallerDetailsLog
    SectionEnd	;; SEC_CreateUninstaller
!macroend

!endif