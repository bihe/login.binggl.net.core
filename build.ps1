cls
echo "Build the login.binggl.net.core Application"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Dist\
dotnet clean -c Debug
dotnet clean -c Release

dotnet build -c Release

dotnet publish .\Login.Web\Login.Web.csproj -c Release -v m -o "$pwd\Dist"
# cleanup/prepare for deployment
Remove-Item -Force .\Dist\appsettings.Development.json
Rename-Item .\Dist\appsettings.json appsettings.json.template