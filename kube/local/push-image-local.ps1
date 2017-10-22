echo "set docker-machine"
docker-machine env default | Invoke-Expression
echo "active docker-machine:"
docker-machine active

echo "tagging image..."
$ver = "2";
$image = "identity";
$registry = "localhost:5000";
$tag = $registry + "/" + $image + ":" + $ver;
docker tag $image $tag;
docker push $tag;