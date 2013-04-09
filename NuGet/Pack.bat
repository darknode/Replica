cd ..\Source
call Compile.bat

cd ..\NuGet

del *.nupkg

NuGet Pack Replica.nuspec
rename Replica.*.nupkg Replica.nupkg
