cls
echo "Build the login.binggl.net.core Application"
#Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Dist\
dotnet clean -c Debug
dotnet clean -c Release

dotnet build -c Release

## cleanup the whole deployment directory
Get-ChildItem -Path  '..\deployment' -Recurse |
	Select -ExpandProperty FullName |
	Where {$_ -notlike '.git*'} |
	sort length -Descending |
	Remove-Item -force

dotnet publish .\Login.Api\Login.Api.csproj -c Release -v m -o "$pwd\..\deployment"

# cleanup/prepare for deployment
Remove-Item -Force ..\deployment\appsettings.Development.json

If (Test-Path ..\deployment\appsettings.json.template){
	Remove-Item ..\deployment\appsettings.json.template
}
Rename-Item ..\deployment\appsettings.json appsettings.json.template
