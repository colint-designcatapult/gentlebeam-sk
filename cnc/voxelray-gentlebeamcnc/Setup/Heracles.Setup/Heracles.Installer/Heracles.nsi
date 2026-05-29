Unicode false

!addplugindir "..\..\Common\NsisPlugins\NSISLog\plugin"
!addplugindir "..\..\Common\NsisPlugins\nsisunz\plugin"
!addplugindir "..\..\Common\NsisPlugins\EnVar_plugin\Plugins"


!addincludedir "..\..\Common\include"
!addincludedir "..\..\Common\include\Functions"

;--------------------------------
; Installer info
;--------------------------------
!define PRODUCT_NAME   "Heracles"
!define COMPANY_NAME   "GentleBeam"
!define VERSION        "1.1.21"
!define OUT_DIR        "bin"
!define INPUT_DIR      "src"
!define SETUP_FILENAME "Heracles-${VERSION}-Setup.exe"

;--------------------------------
; Include system files
;--------------------------------
!include "MUI.nsh"
!include "x64.nsh" 
!include "Sections.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"
!include "StrFunc.nsh"
!include "WinVer.nsh"
;!include "DotNetVersion.nsh"

!include "TextFunc.nsh"
!insertmacro ConfigWriteS

;--------------------------------
; Include setup files
;--------------------------------
!define GIT_ROOT_DIR   '..\..\..'

; Include defines with In/Out folder path
;   must include before other setup nsh-files
!include "include\InputFolders.nsh"
!include "include\OutputFolders.nsh"

; Include all functions and macros
!include "Functions.nsh"


Var /GLOBAL IsWin10
Var /GLOBAL IsRunWithAdminRights

;--------------------------------
; Installer GUI info
;
Name    "${COMPANY_NAME} ${PRODUCT_NAME} ${VERSION}"
Caption "${COMPANY_NAME} ${PRODUCT_NAME} ${VERSION}"

;--------------------------------
;Include Modern UI
;



BrandingText " "

;--------------------------------
; Admin/User rights
; Values:
;    - Admin    - Administrator privileges are required
;    - Power    - Power User privileges are required
;    - Highest  - Request the highest possible execution level for the current user
;    - Standard - No special rights required
!define MULTIUSER_EXECUTIONLEVEL Standard
;!define MULTIUSER_NOUNINSTALL ;Uncomment if no uninstaller is created
!include MultiUser.nsh
;RequestExecutionLevel admin


XPStyle on
WindowIcon on

ShowInstDetails hide
ShowUninstDetails hide

;SetCompressor /SOLID lzma
;SetCompressorDictSize 64

UninstallText "This will completely remove ALL files under the following folder."

!define MUI_ICON "Heracles.ico"
!define HERACLES_INDOOR_EXECUTABLE_NAME		'Heracles.Indoor'
!define HERACLES_OUTDOOR_EXECUTABLE_NAME	'Heracles.External'


; Create build directory
!system 'md "${OUT_DIR}"'
OutFile ${OUT_DIR}\${SETUP_FILENAME}

SetDateSave on
SetDatablockOptimize on
CRCCheck on
SilentInstall normal

InstallDir "${DEFAULT_INSTALL_DIR}\"

#;Function LaunchLink
#;    SetOutPath $INSTDIR\"
#;    ExecShell "" "$DESKTOP\Heracles.lnk"
#;FunctionEnd

;--------------------------------
;Pages
    ; Welcome Page Configuration
    ;!define MUI_WELCOMEPAGE_TITLE 'My test title'
    ;!define MUI_WELCOMEPAGE_TEXT  'My test\nTest text on new line\nAdd empty line\n\nNext text'
    ;!insertmacro MUI_PAGE_WELCOME
    
    !insertmacro MUI_PAGE_LICENSE "src\EndUserLicenseAgreement.txt"
    !insertmacro MUI_PAGE_COMPONENTS
    !insertmacro MUI_PAGE_DIRECTORY
    
    ;Start Menu Folder Page Configuration
    !define MUI_STARTMENUPAGE_REGISTRY_ROOT       "HKCU" 
    !define MUI_STARTMENUPAGE_REGISTRY_KEY        "${STARTMENU_REGISTRY_KEY}" 
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  "Start Menu Folder"
    Var StartMenuFolder
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER       "${PRODUCT_NAME}"
    !define MUI_STARTMENUPAGE_NODISABLE
    !define MUI_PAGE_CUSTOMFUNCTION_PRE CheckSkipStartMenuPage
    !insertmacro MUI_PAGE_STARTMENU "Application" $StartMenuFolder

    !insertmacro MUI_PAGE_INSTFILES

    ;!define MUI_FINISHPAGE_RUN
    ;!define MUI_FINISHPAGE_RUN_TEXT "Start Heracles application."
    ;!define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
        
    !insertmacro MUI_PAGE_FINISH
    
    !insertmacro MUI_UNPAGE_CONFIRM
    !insertmacro MUI_UNPAGE_INSTFILES
  
  
;--------------------------------
;Languages 
!insertmacro MUI_LANGUAGE "English"


;--------------------------------
;           SECTIONS
;--------------------------------
; Include file with all sections macros
;   must include after all other setup nsh-files
!include "include\Sections.nsh"

!insertmacro AddSection_SEC_BackupExistsInstalled
!insertmacro AddSection_SEC_CopyEula
!insertmacro AddSection_SEC_CreateUninstaller

; /e - expanded section group by default
SectionGroup /e "!Programs" SEC_GRP_Programs
    !insertmacro AddSection_SEC_HERACLES_INDOOR
    !insertmacro AddSection_SEC_HERACLES_OUTDOOR
SectionGroupEnd	;; SEC_GRP_Programs


#SectionGroup "!Documentation" SEC_SEC_GRP_DOCS
#    !insertmacro AddSection_SEC_DOC_HERACLES_INDOOR
#    !insertmacro AddSection_SEC_DOC_HERACLES_EXTERNAL
#SectionGroupEnd	;; SEC_SEC_GRP_DOCS

SectionGroup /e "!Shortcuts" SEC_GRP_Shortcuts
    !insertmacro AddSection_SEC_START_MENU_SHORTCUTS
    !insertmacro AddSection_SEC_DESKTOP_SHORTCUTS
SectionGroupEnd	;; SEC_GRP_Shortcuts


; /e - expanded section group by default
;SectionGroup /e "!Dependencies" SEC_GRP_Dependencies
;   !insertmacro AddSection_SEC_VC_REDIST
;SectionGroupEnd	;; SEC_GRP_Dependencies

;--------------------------------
;   SECTIONS TEXT DESCRIPTIONS
;--------------------------------
    ; Descriptions for sections groups
    
    LangString DESC_SEC_GRP_Programs        ${LANG_ENGLISH} "Programs"
    LangString DESC_SEC_SEC_GRP_DOCS        ${LANG_ENGLISH} "Documentation Files"
    LangString DESC_SEC_GRP_Shortcuts       ${LANG_ENGLISH} "Shortcuts"
    LangString DESC_SEC_GRP_Dependencies    ${LANG_ENGLISH} "Dependencies"

    ; Set descriptions text for each Section and SectionGroup
    !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_BackupExistsInstalled}        $(DESC_SEC_BackupExistsInstalled)
        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_CopyEula}                     $(DESC_SEC_CopyEula)
        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_CreateUninstaller}            $(DESC_SEC_CreateUninstaller)
        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_GRP_Programs}                 $(DESC_SEC_GRP_Programs)
            !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_HERACLES_INDOOR}                   $(DESC_SEC_HERACLES_INDOOR)
            !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_HERACLES_OUTDOOR}                  $(DESC_SEC_HERACLES_OUTDOOR)
        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_GRP_Shortcuts}                $(DESC_SEC_GRP_Shortcuts)
            !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_START_MENU_SHORTCUTS}         $(DESC_SEC_START_MENU_SHORTCUTS)
            !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_DESKTOP_SHORTCUTS}            $(DESC_SEC_DESKTOP_SHORTCUTS)
    ;    !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_GRP_Dependencies}             $(DESC_SEC_GRP_Dependencies)
    ;        !insertmacro MUI_DESCRIPTION_TEXT           ${SEC_VC_REDIST}                    $(DESC_SEC_VC_REDIST)
    !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; SECTIONS TEXT DESCRIPTIONS END
;--------------------------------

Section "Uninstall"
    SetDetailsView show
    SetShellVarContext current
        
;; Remove INSTDIR
    RMDir /r "$INSTDIR"
    Sleep 1000    
    RMDir /r "$INSTDIR"
    
;; Delete shortcuts in Start menu entries and regkey with path
    !insertmacro MUI_STARTMENU_GETFOLDER "Application" $R0
    !insertmacro Log "Start remove shrotcuts in StartMenu: $SMPROGRAMS\$R0"
    RMDIR /r "$SMPROGRAMS\$R0"
    
;; Delete shortcuts in Desktop    
    !insertmacro Log "Start remove shrotcuts in Desktop: $DESKTOP"
    Delete "$DESKTOP\Heracles.Indoor.lnk"
    Delete "$DESKTOP\Heracles.External.lnk"
        
;; Remove Registry keys of Current user (HKCU)
    !insertmacro Log "Start remove registry key in HKCU: ${STARTMENU_REGISTRY_KEY}"
    DeleteRegKey HKCU "${STARTMENU_REGISTRY_KEY}"    
    
    !insertmacro Log "Start remove registry key in HKCU: ${ADD_REMOVE_REGISTRY_KEY}"
    DeleteRegKey HKCU "${ADD_REMOVE_REGISTRY_KEY}"
    
    
    SetDetailsPrint textonly
    !insertmacro Log "Completed. You may click Close button."
    SetDetailsPrint none
SectionEnd

Function .onInit
    !insertmacro MULTIUSER_INIT
         
    Call InitInstallTimestampString    
    
    !insertmacro Log "Start log"    
    !insertmacro LogToFile "SETUP_FILENAME: ${SETUP_FILENAME}"
    !insertmacro LogToFile "COMPILE_TIMESTAMP: ${COMPILE_TIMESTAMP}"
    !insertmacro LogToFile "INSTALL_TIMESTAMP: $INSTALL_TIMESTAMP"
        
    ; Check Win10 or higher
    !insertmacro Log "Check Win10 or higher ..."
    ${If} ${AtLeastWin10}    
        StrCpy $IsWin10 'true'
    ${Else}
        StrCpy $IsWin10 'false'
    ${EndIf}
    !insertmacro LogToFile "IsWin10 = $IsWin10"
    
    ; Check admin rights
    !insertmacro Log "Check admin rights ..."
    UserInfo::GetAccountType
    pop $0
    ${If} $0 == "admin"
        StrCpy $IsRunWithAdminRights 'true'
    ${Else}
        StrCpy $IsRunWithAdminRights 'false'
    ${EndIf}
    !insertmacro LogToFile "IsRunWithAdminRights = $IsRunWithAdminRights"
    
    ; Check if .NET Core 8 is installed

    !insertmacro DotNetRuntimeCheck $1 "Microsoft.NETCore.App 8.0"
    
    ${If} $1 = 0    # There's no NETCore 8.0 runtime
        ; No proper runtime:
        MessageBox MB_YESNO|MB_ICONQUESTION "\
            Application cannot be installed.$\r$\n$\r$\n\
            Microsoft .NET Core or ASP.NET 8.0 runtime is not installed yet. \
            Please install the runtime and try again.$\r$\n$\r$\n\
            Open the page to download .NET installers?" IDNO +2 
        ExecShell open "https://dotnet.microsoft.com/download/dotnet/8.0"
        Quit
    ${EndIf}
    
    ; Check Installed .NET (only 4.5 or higher)
    ;ClearErrors
    ;; Magic numbers from http://msdn.microsoft.com/en-us/library/ee942965.aspx
    ;ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"    
    ;IfErrors NotDetected    
    ;MessageBox MB_OK "Microsoft .NET Framework version is ($0)"
    ;${If} $0 >= 378389
    ;    !insertmacro Log "Microsoft .NET Framework 4.0 is installed ($0)"
    ;    MessageBox MB_OK "Microsoft .NET Framework 4.0 is installed ($0)"
    ;${Else}
    ;NotDetected:
    ;    MessageBox MB_OK ".NET Framework not detected"
    ;    ;MessageBox MB_YESNO|MB_ICONQUESTION ".NET Framework 4.5+ is required for ProgramX2013, \
    ;    ;    do you want to launch the web installer? This requires a valid internet connection." IDYES InstallDotNet IDNO Cancel 
    ;    ;Cancel:
    ;    ;    MessageBox MB_ICONEXCLAMATION "To install ProgramX, Microsoft's .NET Framework v${DOT_MAJOR}.${DOT_MINOR} \
    ;    ;        (or higher) must be installed. Cannot proceed with the installation!"
    ;    ;    ${OpenURL} "${WWW_MS_DOTNET4_5}"
    ;    ;    RMDir /r "$INSTDIR" 
    ;    ;    SetOutPath "$PROGRAMFILES"
    ;    ;    RMDir "$INSTDIR" 
    ;    ;    Abort
    ;    ;
    ;    ;; Install .NET4.5.
    ;    ;InstallDotNet:
    ;    ;    !insertmacro Log "Installing Microsoft .NET Framework 4.5"
    ;    ;    SetDetailsPrint listonly
    ;    ;    ExecWait '"$INSTDIR\dotNETFramework\dotNetFx45_Full_setup.exe" /passive /norestart' $0
    ;    ;    ${If} $0 == 3010 
    ;    ;    ${OrIf} $0 == 1641
    ;    ;        !insertmacro Log "Microsoft .NET Framework 4.5 installer requested reboot."
    ;    ;        SetRebootFlag true 
    ;    ;    ${EndIf}
    ;    ;    SetDetailsPrint lastused
    ;    ;    !insertmacro Log "Microsoft .NET Framework 4.5 installer returned $0"
    ;${EndIf}

    ; Now remove the dotNETFramework directory and contents.
    ;RMDir /r "$INSTDIR\dotNETFramework" 
     
    
    ; KILL all our processes
    ; taskkill /IM "process name" /F
    !insertmacro LogToFile "Start kill all our processes"
    !insertmacro KillProcess "Heracles.Indoor.exe"  
    !insertmacro KillProcess "Heracles.External.exe"  
    
    ; Set defaults variables
    !insertmacro ResetToDefaultShortcutVars
    
    ; Dump log-text (ShowDetail) to text file (for current user)
    Call DumpInstallerDetailsLog
FunctionEnd

Function .onSelChange
    ; Check changed checkbox "Start Menu shortcuts"
    ${If} $0 = ${SEC_START_MENU_SHORTCUTS}
        ${If} ${SectionIsSelected} ${SEC_START_MENU_SHORTCUTS}
            ;MessageBox MB_ICONEXCLAMATION "Section SEC_START_MENU_SHORTCUTS selected! Old value IsSelectedSecStartMenuShortcuts=$IsSelectedSecStartMenuShortcuts"
            StrCpy $IsSelectedSecStartMenuShortcuts 'true'
        ${Else}
            ;MessageBox MB_ICONEXCLAMATION "Section SEC_START_MENU_SHORTCUTS unselected! Old value IsSelectedSecStartMenuShortcuts=$IsSelectedSecStartMenuShortcuts"
            StrCpy $IsSelectedSecStartMenuShortcuts 'false'
        ${EndIf}
    ${EndIf}
    
    ; Check changed checkbox "Desktop shortcuts"
    ${If} $0 = ${SEC_DESKTOP_SHORTCUTS}
        ${If} ${SectionIsSelected} ${SEC_DESKTOP_SHORTCUTS}
            ;MessageBox MB_ICONEXCLAMATION "Section SEC_DESKTOP_SHORTCUTS selected! Old value IsSelectedSecDesktopShortcuts=$IsSelectedSecDesktopShortcuts"
            StrCpy $IsSelectedSecDesktopShortcuts 'true'
        ${Else}
            ;MessageBox MB_ICONEXCLAMATION "Section SEC_DESKTOP_SHORTCUTS unselected! Old value IsSelectedSecDesktopShortcuts=$IsSelectedSecDesktopShortcuts"
            StrCpy $IsSelectedSecDesktopShortcuts 'false'
        ${EndIf}
    ${EndIf}

FunctionEnd

Function .onInstFailed
    !insertmacro Log "Install Failed or Aborted"    
    ; Dump log-text (ShowDetail) to text file (for current user)
    Call DumpInstallerDetailsLog
FunctionEnd

Function .onInstSuccess    
    !insertmacro Log "Install Success"    
    ; Dump log-text (ShowDetail) to text file (for current user)
    Call DumpInstallerDetailsLog
FunctionEnd

Function un.onInit
    !insertmacro MULTIUSER_UNINIT
FunctionEnd
