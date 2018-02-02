$project_name = "NLight"

cd $project_name\bin\release

$pkg = dir *.nupkg | Select-Object -Last 1

Write-Host $pkg

dotnet nuget push $pkg -s "https://api.nuget.org/v3/index.json"

pause