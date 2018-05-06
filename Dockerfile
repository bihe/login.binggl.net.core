FROM microsoft/dotnet:2.1-aspnetcore-runtime-bionic
LABEL author="henrik@binggl.net"

WORKDIR /opt/login.binggl.net

RUN mkdir -p /opt/login.binggl.net/_logs && mkdir -p /opt/login.binggl.net/_db && mkdir -p /opt/login.binggl.net/_config/Production
COPY ./dist/ /opt/login.binggl.net/
RUN ls -la /opt/login.binggl.net/

EXPOSE 3000

ENTRYPOINT ["dotnet", "Login.Api.dll"]
