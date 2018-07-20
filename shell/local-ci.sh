#!/bin/sh
set -e
TAG=$(date +"%Y-%m-%d--%H-%M-%S")
PROJ=identity
REGISTRY=localhost:5000
IMAGE=$REGISTRY/$PROJ:$TAG
eval $(docker-machine env default --shell bash)
docker build -t $IMAGE --build-arg DOTNET_CONFIG=Build ../app
kubectl config set-context minikube
docker push $IMAGE
    # -s env.PORTAL_URL=http://localhost:7000 \
# ./helm-deploy.sh -i postgres -r iddb -c ../kube/iddb
./helm-deploy.sh -i $IMAGE -r $PROJ -c ../kube/$PROJ \
    -s env.ASPNETCORE_ENVIRONMENT=Development \
    -s env.PORTAL_URL=http://192.168.1.35:31067 \
