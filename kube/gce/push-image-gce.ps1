echo "set docker-machine"
docker-machine env default | Invoke-Expression
echo "active docker-machine:"
docker-machine active

echo "tagging image..."
$ver = "8";
$image = "identity";
$registry = "us.gcr.io/midnight-lizard";
$tag = $registry + "/" + $image + ":" + $ver;
docker tag $image $tag;
gcloud docker -- push $tag;