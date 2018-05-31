. .\_environment.ps1

docker stop login
docker rm login

docker run -d -p 3000:3000 --name login `
  -v d:/Development/login.binggl.net/application/Login.Api/_db:/opt/login.binggl.net/_db `
  -e "ASPNETCORE_ENVIRONMENT=Production" `
  -e "ASPNETCORE_URLS=http://*:3000" `
  -e ConnectionStrings:LoginConnection `
  -e Application:BaseUrl `
  -e Application:Authentication:CookieDomain `
  -e Application:Authentication:GoogleClientId `
  -e Application:Authentication:GoogleClientSecret `
  -e Application:ApplicationSalt `
  -e Application:Jwt.TokenSecret `
login
