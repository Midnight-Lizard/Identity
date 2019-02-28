pod=$(kubectl get pods --selector=app=iddb -o jsonpath='{.items[0].metadata.name}');

kubectl port-forward $pod 5433:5432