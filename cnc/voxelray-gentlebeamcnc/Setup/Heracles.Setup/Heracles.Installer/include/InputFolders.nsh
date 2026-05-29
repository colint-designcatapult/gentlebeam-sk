; Required defines before include this file:
; !define INPUT_DIR "..."
; !define GIT_ROOT_DIR "..."

!ifndef SETUP_FUNCTIONS_INPUT_FOLDERS_NSH
!define SETUP_FUNCTIONS_INPUT_FOLDERS_NSH

;--------------------------------
; Input/Source directories
;--------------------------------
!define HERACLES_SETUP_DIR                    '${GIT_ROOT_DIR}\Setup\'

; ACE-C-Viewer directories
!define HERACLES_SLN_DIR                      '${GIT_ROOT_DIR}\'
!define HERACLES_INDOOR_BIN_64_DIR            '${HERACLES_SLN_DIR}Xcc\Heracles\Heracles.Indoor\bin\x64\Release\net8.0-windows7.0'
!define HERACLES_OUTDOOR_BIN_64_DIR           '${HERACLES_SLN_DIR}Xcc\Heracles\Heracles.Outdoor\bin\x64\Release\net8.0-windows7.0'

; Dependencies directories
!define DEPENDENCIES_DIR                 '${INPUT_DIR}\Dependencies\'

!endif