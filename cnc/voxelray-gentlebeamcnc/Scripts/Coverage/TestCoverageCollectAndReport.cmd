@echo off

SET scriptPath=%~dp0
rem cd %scriptPath%

rem We probably need to provide a way to specify this path from outside. Now it defaults to <xcc_root>\CoverageReports
SET xccRootFolder=%scriptPath%..\..\
cd %xccRootFolder%

SET ddmmyyyy=%date:.=%
SET datestamp=%ddmmyyyy:~4,4%%ddmmyyyy:~2,2%%ddmmyyyy:~0,2%
SET timestring=%time: =0%
SET timestamp=%timestring::=%

SET workingDir=CoverageReports\Report_%datestamp%_%timestamp:~0,6%

echo %workingDir%

mkdir %workingDir%
rem dotnet test --collect:"XPlat Code Coverage;Format=cobertura" --results-directory %workingDir% --settings %scriptPath%\Coverage.runsettings
dotnet test --filter FullyQualifiedName\!~IntegrationTest --collect:"XPlat Code Coverage;Format=cobertura" --results-directory %workingDir%
reportgenerator "-reports:%workingDir%/*/coverage.cobertura.xml" "-reporttypes:Html" "-targetdir:%workingDir%/html" "-classfilters:-Com.Empyreanmed.*;-Protos.*;-Xcc.Core.Protos.*;-*.Dummy*;-Xcc.Test.*"

@echo on