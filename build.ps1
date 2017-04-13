cls
echo "Build the login.binggl.net.core Application"
#Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Dist\
dotnet clean -c Debug
dotnet clean -c Release

$ts = Get-Date -format "yyyyMMdd"
echo $ts

dotnet build --version-suffix $ts -c Release 

dotnet publish .\Login.Web\Login.Web.csproj -c Release -v m -o "$pwd\..\deployment"
# cleanup/prepare for deployment
Remove-Item -Force ..\deployment\appsettings.Development.json

If (Test-Path ..\deployment\appsettings.json.template){
	Remove-Item ..\deployment\appsettings.json.template
}
Rename-Item ..\deployment\appsettings.json appsettings.json.template