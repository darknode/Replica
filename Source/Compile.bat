%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /target:Clean Replica\Replica.csproj /property:Configuration=Debug
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /target:Clean Replica\Replica.csproj /property:Configuration=Release
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Replica\Replica.csproj /property:Configuration=Debug
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Replica\Replica.csproj /property:Configuration=Release

pause