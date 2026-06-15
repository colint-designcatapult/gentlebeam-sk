@echo off

setlocal enabledelayedexpansion

rem Get target commit hash string from file
set workingDir=%cd%
set commit_file=%workingDir%\all_deps_actual_commits.txt

for /f "tokens=1,2,3 delims=:" %%a in (%commit_file%) do (
  set project_name=%%a
  set branch=%%b
  set commit=%%c
  echo Checkout project "!project_name!" to commit !commit! on the branch !branch!
  set repoRootDir=../..
  set projectDir=!repoRootDir!/!project_name!
  if not exist !projectDir! (
	echo "No project folder for !project_name!, try to clone the repo"
	git clone "https://bitbucket.org/empyrean-medical-devices/!project_name!.git" !projectDir!
  )
  if exist !projectDir! (
    echo goto !projectDir!
    call ./VersionManagement/repo_commit_checkout.cmd !projectDir! !branch! !commit!
    @echo off
    echo.
  )
)

@echo on