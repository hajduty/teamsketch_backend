Write-Host "Setting context to Minikube..."
kubectl config use-context minikube

Write-Host "`nDeploying SQL Server..."
kubectl apply -f ./k8s/sqlserver-deployment.yaml
kubectl apply -f ./k8s/sqlserver-service.yaml

Write-Host "`nDeploying AuthService..."
kubectl apply -f ./k8s/authservice-deployment.yaml
kubectl apply -f ./k8s/authservice-service.yaml

Write-Host "`nDeploying PermissionService..."
kubectl apply -f ./k8s/permissionservice-deployment.yaml
kubectl apply -f ./k8s/permissionservice-service.yaml

Write-Host "`nDeploying UserService..."
kubectl apply -f ./k8s/userservice-deployment.yaml
kubectl apply -f ./k8s/userservice-service.yaml

Write-Host "`nDeploying RoomService..."
kubectl apply -f ./k8s/roomservice-deployment.yaml
kubectl apply -f ./k8s/roomservice-service.yaml

Write-Host "`nAll deployments applied successfully!"

Write-Host "`nWaiting for pods to be ready..."
kubectl wait --for=condition=Ready pods --all --timeout=120s

kubectl get pods
kubectl get svc
