#!/bin/sh
set -e
TAG=$(date +"%Y-%m-%d--%H-%M-%S")
PROJ=identity
REGISTRY=localhost:5000
IMAGE=$REGISTRY/$PROJ:$TAG
eval $(docker-machine env default --shell bash)
docker build -t $IMAGE --build-arg DOTNET_CONFIG=Build ../app
kubectl config use-context minikube
docker push $IMAGE
# ./helm-deploy.sh -i debezium/postgres:10.0 -r iddb -c ../kube/iddb

secret=$(echo -n not-a-secret | base64 -w 0);
IDENTITY_SERVER_SIGNING_CERTIFICATE=$(<../cert/signing-certificate.txt);
IDENTITY_SERVER_SIGNING_CERTIFICATE_PASSWORD=$secret;
IDENTITY_GOOGLE_CLIENT_ID=$secret;
IDENTITY_GOOGLE_CLIENT_SECRET=$secret;
IDENTITY_PORTAL_CLIENT_SECRET=$secret;
IDENTITY_SCHEMES_COMMANDER_API_SECRET=$secret;
IDENTITY_SCHEMES_QUERIER_API_SECRET=$secret;
IDENTITY_OWNER_EMAILS_JSON_ARRAY=$(echo -n '["test@user.com","test@admin.com"]' | base64 -w 0);

./helm-deploy.sh -i $IMAGE -r $PROJ -c ../kube/$PROJ \
    --set env.ASPNETCORE_ENVIRONMENT=Development \
    --set secrets.signingCertificate.data=$IDENTITY_SERVER_SIGNING_CERTIFICATE \
    --set secrets.signingCertificate.password=$IDENTITY_SERVER_SIGNING_CERTIFICATE_PASSWORD \
    --set secrets.google.clientId=$IDENTITY_GOOGLE_CLIENT_ID \
    --set secrets.google.clientSecret=$IDENTITY_GOOGLE_CLIENT_SECRET \
    --set secrets.portal.clientSecret=$IDENTITY_PORTAL_CLIENT_SECRET \
    --set secrets.schemesCommander.apiSecret=$IDENTITY_SCHEMES_COMMANDER_API_SECRET \
    --set secrets.schemesQuerier.apiSecret=$IDENTITY_SCHEMES_QUERIER_API_SECRET \
    --set secrets.owners.emailsJsonArray=$IDENTITY_OWNER_EMAILS_JSON_ARRAY \
    --set env.PORTAL_URL=http://192.168.1.44:31565 \
    --set env.IDENTITY_URL=http://192.168.1.44:32006 \
    --set env.IDENTITY_PORTAL_ACCESS_TOKEN_LIFETIME=2147483647 \
