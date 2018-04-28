#!/bin/bash
dotnet watch --project Login.XTests.csproj test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info
