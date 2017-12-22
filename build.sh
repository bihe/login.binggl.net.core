#!/bin/sh

clear
echo "Build the login.binggl.net.core Application"
dotnet clean -c Debug
dotnet clean -c Release

ts=`date +%Y%m%d`
echo $ts

dotnet build --version-suffix $ts -c Release 

## cleanup the whole deployment directory
rm -rf $PWD/../deployment/* 

dotnet publish ./Login.Web/Login.Web.csproj -c Release -v m -o $PWD/../deployment

# cleanup/prepare for deployment
rm -f $PWD/../deployment/appsettings.Development.json
rm -f $PWD/../deployment/appsettings.json.template
mv $PWD/../deployment/appsettings.json $PWD/../deployment/appsettings.json.template