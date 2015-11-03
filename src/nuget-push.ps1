$project_name = "NLight"

cd $project_name

del *.nupkg -Force

..\.nuget\nuget.exe pack ($project_name + ".csproj") -Properties Configuration=Release -Build -IncludeReferencedProjects -NonInteractive

$pkg = dir *.nupkg | Select-Object -Last 1

Write-Host $pkg

..\.nuget\nuget.exe push $pkg

pause