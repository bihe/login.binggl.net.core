cls
echo "Build the login.binggl.net.core Application"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Dist\
dotnet clean -c Debug
dotnet clean -c Release

dotnet build -c Release

dotnet publish .\Login.Web\Login.Web.csproj -c Release -v m -o "$pwd\Dist"