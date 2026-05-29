!ifndef SETUP_SECTIONS_PROGRAMS_SEC_HERACLES_INDOOR_NSH
!define SETUP_SECTIONS_PROGRAMS_SEC_HERACLES_INDOOR_NSH

LangString DESC_SEC_HERACLES_INDOOR  ${LANG_ENGLISH} "Heracles Indoor Application"

!macro AddSection_SEC_HERACLES_INDOOR
    Section "Heracles.Indoor" SEC_HERACLES_INDOOR
    ;; Start Section
        SetDetailsView show 
        !insertmacro Log "Start install section: SEC_HERACLES_INDOOR"
            
        SetShellVarContext current
        		
        !insertmacro Log "Extract Heracles.Indoor files. Path: ${HERACLES_INDOOR_INST_DIR}"
        SetOutPath          "${HERACLES_INDOOR_INST_DIR}"
		
		;; HERACLES INDOOR files
		; Copy executables
        !insertmacro FileEx "${HERACLES_INDOOR_BIN_64_DIR}\${HERACLES_INDOOR_EXECUTABLE_NAME}.exe"
		
		; Copy libraries
		!insertmacro FileEx "${HERACLES_INDOOR_BIN_64_DIR}\*.dll"

		; Copy config files
		!insertmacro FileEx "${HERACLES_INDOOR_BIN_64_DIR}\*.runtimeconfig.json"
		!insertmacro FileEx "${HERACLES_INDOOR_BIN_64_DIR}\*.deps.json"

        ; Copy deps for ScottPlot used in PhotoAcoustic:
        ;File /r "${HERACLES_INDOOR_BIN_64_DIR}\glfw3.dll"
        ;File /r "${HERACLES_INDOOR_BIN_64_DIR}\libSkiaSharp.dll"

		!insertmacro FileEx  "${HERACLES_SLN_DIR}\Xcc\Heracles\Config\Indoor\appsettings.json"

		;; Create shortcuts
        !insertmacro MUI_STARTMENU_WRITE_BEGIN "Application"
        !insertmacro CreateStartMenuShortcut "${HERACLES_INDOOR_INST_DIR}"  "${HERACLES_INDOOR_EXECUTABLE_NAME}.exe"  "${HERACLES_INDOOR_EXECUTABLE_NAME}.lnk"
        !insertmacro MUI_STARTMENU_WRITE_END
            
        !insertmacro CreateDesktopShortcut   "${HERACLES_INDOOR_INST_DIR}"  "${HERACLES_INDOOR_EXECUTABLE_NAME}.exe"  "${HERACLES_INDOOR_EXECUTABLE_NAME}.lnk"


    ;; End Section		
        !insertmacro Log "End install section: SEC_HERACLES_INDOOR"
        ; Dump log-text (ShowDetail) to text file (for current user)
        Call DumpInstallerDetailsLog
    SectionEnd	;; SEC_HERACLES_INDOOR
!macroend

!endif