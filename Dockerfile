## backend build-phase
FROM microsoft/dotnet:2.1-sdk AS BACKEND-BUILD
COPY ./Login.Api ./backend-build
WORKDIR /backend-build
RUN dotnet build -c Release && dotnet publish Login.Api.csproj -c Release -v m -o output



## runtime build
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
LABEL author="henrik@binggl.net"
WORKDIR /opt/login.binggl.net
RUN mkdir -p /opt/login.binggl.net/_logs && mkdir -p /opt/login.binggl.net/_db && mkdir -p /opt/login.binggl.net/_config
COPY --from=BACKEND-BUILD /backend-build/output /opt/login.binggl.net/
RUN ls -la /opt/login.binggl.net/
EXPOSE 3000
ENTRYPOINT ["dotnet", "Login.Api.dll"]
