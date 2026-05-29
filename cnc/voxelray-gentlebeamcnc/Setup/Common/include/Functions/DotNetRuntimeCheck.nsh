!ifndef SETUP_FUNCTIONS_DOTNET_RUNTIME_CHECK_NSH
!define SETUP_FUNCTIONS_DOTNET_RUNTIME_CHECK_NSH

###########################################################################
# Call dotnet to get list of runtimes and look for a necessary prefix there
###########################################################################
!macro DotNetRuntimeCheck return_var runtimeNamePrefix
    # we also call 'cmd /c dir .' as a workaround
    # as ExecToStack doesn't pipe non-native dotnet command to find command properly 
    nsExec::ExecToStack /OEM 'cmd /c dir . | dotnet --list-runtimes | find /c /i "${runtimeNamePrefix}"'
    Pop $0 # ignore the exit code/text
    Pop ${return_var} # output the number of matches
!macroend

!endif