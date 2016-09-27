$project_name = "NLight"

cd $project_name

del *.nupkg -Force

..\.paket\paket.exe pack output .

$pkg = dir *.nupkg | Select-Object -Last 1

Write-Host $pkg

..\.paket\paket.exe push url "https://www.nuget.org" file $pkg

pause