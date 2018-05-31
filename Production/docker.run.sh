#!/bin/bash
docker pull bihe/login
docker run --restart unless-stopped -d -p 127.0.0.1:3003:3000 --user <USER-ID> \
  --name login-app \
  --network=app-network \
  -v /var/www/docker/login/logs:/opt/login.binggl.net/_logs \
  -v /var/www/docker/login/db:/opt/login.binggl.net/_db \
  -v /var/www/docker/login/etc:/opt/login.binggl.net/_config \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  -e "ASPNETCORE_URLS=http://*:3000" \
  -e "ASPNETCORE_HTTPS_PORT=443" \
bihe/login
