!ifndef SETUP_SECTIONS_PROGRAMS_SEC_HERACLES_OUTDOOR_NSH
!define SETUP_SECTIONS_PROGRAMS_SEC_HERACLES_OUTDOOR_NSH

LangString DESC_SEC_HERACLES_OUTDOOR  ${LANG_ENGLISH} "Heracles External Application"

!macro AddSection_SEC_HERACLES_OUTDOOR
    Section "Heracles.External" SEC_HERACLES_OUTDOOR
    ;; Start Section
        SetDetailsView show 
        !insertmacro Log "Start install section: SEC_HERACLES_OUTDOOR"
            
        SetShellVarContext current
        		
        !insertmacro Log "Extract Heracles.Outdoor files. Path: ${HERACLES_OUTDOOR_INST_DIR}"
        SetOutPath          "${HERACLES_OUTDOOR_INST_DIR}"
		
		;; XCC OUTDOOR files
		; Copy executables
        !insertmacro FileEx "${HERACLES_OUTDOOR_BIN_64_DIR}\${HERACLES_OUTDOOR_EXECUTABLE_NAME}.exe"
		
		; Copy libraries
		!insertmacro FileEx "${HERACLES_OUTDOOR_BIN_64_DIR}\*.dll"
				
		; Copy config files
		!insertmacro FileEx  "${HERACLES_OUTDOOR_BIN_64_DIR}\*.runtimeconfig.json"
		!insertmacro FileEx  "${HERACLES_OUTDOOR_BIN_64_DIR}\*.deps.json"

		!insertmacro FileEx  "${HERACLES_SLN_DIR}\Xcc\Heracles\Config\External\appsettings.json"

		;; Create shortcuts
        !insertmacro MUI_STARTMENU_WRITE_BEGIN "Application"
        !insertmacro CreateStartMenuShortcut "${HERACLES_OUTDOOR_INST_DIR}"  "${HERACLES_OUTDOOR_EXECUTABLE_NAME}.exe"  "${HERACLES_OUTDOOR_EXECUTABLE_NAME}.lnk"
        !insertmacro MUI_STARTMENU_WRITE_END
            
        !insertmacro CreateDesktopShortcut   "${HERACLES_OUTDOOR_INST_DIR}"  "${HERACLES_OUTDOOR_EXECUTABLE_NAME}.exe"  "${HERACLES_OUTDOOR_EXECUTABLE_NAME}.lnk"


    ;; End Section		
        !insertmacro Log "End install section: SEC_HERACLES_OUTDOOR"
        ; Dump log-text (ShowDetail) to text file (for current user)
        Call DumpInstallerDetailsLog
    SectionEnd	;; SEC_HERACLES_OUTDOOR
!macroend

!endif