# Code coverage & metrics utilities
## Setup

Install MS dotnet report generator tool:

```
dotnet tool install -g dotnet-reportgenerator-globaltool
```

In case of installation problems (Unhandled exception: System.Net.Http.HttpRequestException: Response status code does not indicate success: 401 (Unauthorized).)
temporarily disable additional nuget sources in solution exept for nuget.org. 

For details, see https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=windows#generate-reports


## Run

To get test coverage report, run the script ```<xcc_root>/Scripts/Coverage/TestCoverageCollectAndReport.cmd```
This will create a new report folder ```<xcc_root>/CoverageReports/Report_YYYYMMDD_HHMMSS```
This folder contains a set of project report subfolders with xml reports and ```html``` subfolder with the integral coverage report in nice html format.
Open ```<report_folder>/html/index.html``` file to explore the report.
