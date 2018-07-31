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
# ./helm-deploy.sh -i debezium/postgres:10.0 -r iddb -c ../kube/iddb
secret=$(echo -n not-a-secret | base64 -w 0);
IDENTITY_SERVER_SIGNING_CERTIFICATE=$(<../cert/signing-certificate.txt);
IDENTITY_SERVER_SIGNING_CERTIFICATE_PASSWORD=$secret;
IDENTITY_GOOGLE_CLIENT_ID=$secret;
IDENTITY_GOOGLE_CLIENT_SECRET=$secret;
IDENTITY_PORTAL_CLIENT_SECRET=$secret;
./helm-deploy.sh -i $IMAGE -r $PROJ -c ../kube/$PROJ \
    --set env.ASPNETCORE_ENVIRONMENT=Development \
    --set secrets.signingCertificate.data=$IDENTITY_SERVER_SIGNING_CERTIFICATE \
    --set secrets.signingCertificate.password=$IDENTITY_SERVER_SIGNING_CERTIFICATE_PASSWORD \
    --set secrets.google.clientId=$IDENTITY_GOOGLE_CLIENT_ID \
    --set secrets.google.clientSecret=$IDENTITY_GOOGLE_CLIENT_SECRET \
    --set secrets.portal.clientSecret=$IDENTITY_PORTAL_CLIENT_SECRET \
    --set env.PORTAL_URL=http://192.168.1.35:31565 \
    # --set env.PORTAL_URL=http://localhost:7000 \
