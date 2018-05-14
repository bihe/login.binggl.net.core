. .\_environment.ps1

docker tag login $env:DOCKER_ID_USER/login
docker push $env:DOCKER_ID_USER/login
