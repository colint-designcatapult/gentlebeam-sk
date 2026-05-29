!ifndef SETUP_SECTIONS_SEC_BACKUP_EXIST_INSTALLATION_NSH
!define SETUP_SECTIONS_SEC_BACKUP_EXIST_INSTALLATION_NSH

LangString DESC_SEC_BackupExistsInstalled   ${LANG_ENGLISH} "Backup exists installed files"

!macro AddSection_SEC_BackupExistsInstalled
    ; Hidden section from GUI ('-' symbol in section name)
    Section "-Backup" SEC_BackupExistsInstalled
    ;; Start Section
        SetDetailsView show 
        !insertmacro Log "Start install section: SEC_BackupExistsInstalled"
            
        SetShellVarContext current

        ; ReadOnly section
        SectionIn RO
        
    ;; Check exist installed Heracles
        !insertmacro Log "Check if there is an installed Heracles version. Path: $INSTDIR\*"
        ${If} ${FileExists} "$INSTDIR\*"        
            Var /GLOBAL BackupName
            StrCpy $BackupName "$INSTALL_TIMESTAMP_${PRODUCT_NAME}"
            
            ; Get file version from Heracles Indoor or Outdoor.exe
            ${GetFileVersion} "${HERACLES_INDOOR_INST_DIR}\${HERACLES_INDOOR_EXECUTABLE_NAME}.exe" $R0
            ${If} $R0 != ""
                StrCpy $BackupName "$BackupName_v$R0"
            ${EndIf} 
            ${GetFileVersion} "${HERACLES_OUTDOOR_INST_DIR}\${HERACLES_OUTDOOR_EXECUTABLE_NAME}.exe" $R0
            ${If} $R0 != ""
                StrCpy $BackupName "$BackupName_v$R0"
            ${EndIf} 
        
            ; Uncomment for debug
            ;MessageBox MB_YESNO `"$INSTDIR" already exists. BackupName: "$BackupName", BACKUP_DIR: "${BACKUP_DIR}`
            
            ; Copy files to backup dir     
            !insertmacro Log "Copy files to backup directory. Path: ${BACKUP_DIR}\$BackupName"
            CreateDirectory          "${BACKUP_DIR}\$BackupName"
            CopyFiles "$INSTDIR\*.*" "${BACKUP_DIR}\$BackupName"
        ${EndIf}  
                
    ;; End Section		
        !insertmacro Log "End install section: SEC_BackupExistsInstalled"
        ; Dump log-text (ShowDetail) to text file (for current user)
        Call DumpInstallerDetailsLog
    SectionEnd	;; SEC_BackupExistsInstalled
!macroend

!endif