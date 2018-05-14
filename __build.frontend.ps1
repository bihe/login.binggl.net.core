cd ./Login.Ui

Write-Host "INFO: Clear the dist folder"  -ForegroundColor Yellow
remove-item -path .\dist\*

yarn install
npm run build -- --prod --bh /ui/
cd ../
