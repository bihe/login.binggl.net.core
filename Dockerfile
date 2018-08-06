## fronted build-phase
FROM node:10-alpine AS FRONTED-BUILD
WORKDIR ./fronted-build
COPY ./Login.Ui .
RUN npm install -g @angular/cli@latest && npm install && npm run build -- --prod --base-href /ui/

## backend build-phase
FROM microsoft/dotnet:2.1-sdk AS BACKEND-BUILD
WORKDIR /backend-build
COPY ./Login.Api .
RUN dotnet build -c Release && dotnet publish Login.Api.csproj -c Release -v m -o output

## runtime build
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
LABEL author="henrik@binggl.net"
LABEL description="The central authentication/authorization endpoint for all binggl.net application"
LABEL version=2
WORKDIR /opt/login.binggl.net
RUN mkdir -p /opt/login.binggl.net/_logs && mkdir -p /opt/login.binggl.net/_db && mkdir -p /opt/login.binggl.net/_config
COPY --from=BACKEND-BUILD /backend-build/output /opt/login.binggl.net/
COPY --from=FRONTED-BUILD /fronted-build/dist /opt/login.binggl.net/wwwroot/ui
RUN ls -la /opt/login.binggl.net/
RUN ls -la /opt/login.binggl.net/wwwroot/ui
EXPOSE 3000
ENTRYPOINT ["dotnet", "Login.Api.dll"]
