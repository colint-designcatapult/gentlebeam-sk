@echo off

rem Get target commit hash string from file
SET workingDir=%cd%
set commit_file=%workingDir%/protos_actual_commit.txt
set /p commit=<%commit_file%

set protosDir="../../protos"
call ./VersionManagement/repo_commit_checkout.cmd %protosDir% dev %commit%

@echo on