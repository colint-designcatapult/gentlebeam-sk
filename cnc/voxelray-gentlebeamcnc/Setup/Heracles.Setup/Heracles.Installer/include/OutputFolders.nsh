; Required defines before include this file:
; !define PRODUCT_NAME "..."
; !define COMPANY_NAME "..."

!ifndef SETUP_FUNCTIONS_OUTPUT_FOLDERS_NSH
!define SETUP_FUNCTIONS_OUTPUT_FOLDERS_NSH

;--------------------------------
; Output/Installation directories
;--------------------------------
!define DEFAULT_INSTALL_DIR          	'C:\${COMPANY_NAME}\${PRODUCT_NAME}'
!define BACKUP_DIR                   	'C:\${COMPANY_NAME}\Backup'

!define HERACLES_INDOOR_INST_DIR     	 	'$INSTDIR\Heracles.Indoor\'
!define HERACLES_OUTDOOR_INST_DIR	        '$INSTDIR\Heracles.External\'

!define LOG_INSTALL_DIR              	'$DOCUMENTS\${COMPANY_NAME}\SetupLogs'

!define STARTMENU_REGISTRY_KEY       	'Software\${PRODUCT_NAME}'
!define ADD_REMOVE_REGISTRY_KEY      	'Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}'

!endif