@echo off
rem Usage: repo_commit_checkout <pathToRepo> <commit>

set workingDir=%cd%
set pathToRepo=%1
set branch=%2
set commit=%3

set git_commit_info=

set git_command=git show %commit%

rem Go to repo folder and start acting on git
cd %pathToRepo%
set firstTry=true

:CommitSearch
for /f "tokens=*" %%f in ('%git_command%') do (set "GIT_COMMIT_INFO=%%f" & goto :CheckCommit)

:CheckCommit
rem Check if the first line of commit info starts with commit (followed by hash)
if defined GIT_COMMIT_INFO (
  if "%GIT_COMMIT_INFO:~0,6%"=="commit" (
    goto :Checkout
  )
)

rem If first attempt was unsuccessful, fetch from origin and try again
if %firstTry% == true (
  echo No such local commit, fetch origin...
  git checkout %branch%
  git pull 
rem  git fetch origin %branch%
  git checkout %branch%
  echo Try to find the commit again
  set firstTry=false
  goto :CommitSearch
) else (
  echo Error: no such commit in fetched data
  exit
)

rem Here we have successfully fetched the desired commit, so checkout to it
:Checkout
echo Checkout to commit %commit%
if defined GIT_COMMIT_INFO (
  echo Commit info: %GIT_COMMIT_INFO%
)
git checkout %commit%

rem Go back to working directory
cd %workingDir%
@echo on