cls
Write-Host "INFO: Build the login.binggl.net.core Application" -ForegroundColor Yellow
#Remove-Item -Recurse -Force -ErrorAction SilentlyContinue .\Dist\
dotnet clean -c Debug
dotnet clean -c Release

Write-Host "INFO: Copy ui build to wwwroot/ui"  -ForegroundColor Yellow
# check existance of files

if (!(Test-Path .\Login.Ui\dist\index.html)) {
  Write-Host "[ERROR] Ui build not available!" -ForegroundColor Red
  exit
}

# clean ui
remove-item -path .\Login.Api\wwwroot\ui\*
# copy new build
copy-item -path .\Login.Ui\dist\* -Destination .\Login.Api\wwwroot\ui\

dotnet build -c Release

## cleanup the whole deployment directory
Get-ChildItem -Path  '.\dist' -Recurse |
	Select -ExpandProperty FullName |
	Where {$_ -notlike '.git*'} |
	sort length -Descending |
	Remove-Item -force

dotnet publish .\Login.Api\Login.Api.csproj -c Release -v m -o "$pwd\dist"
