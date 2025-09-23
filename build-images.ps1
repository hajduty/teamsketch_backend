Write-Host "Configuring Docker to use Minikube..."
minikube -p minikube docker-env | Invoke-Expression

Write-Host "`nBuilding PermissionService..."
docker build -f PermissionService/Dockerfile -t permissionservice:1.0 .

Write-Host "`nBuilding UserService..."
docker build -f UserService/Dockerfile -t userservice:1.0 .

Write-Host "`nBuilding RoomService..."
docker build -f RoomService/Dockerfile -t roomservice:1.0 .

Write-Host "`nBuilding AuthService..."
docker build -f AuthService/Dockerfile -t authservice:1.0 .

Write-Host "`nAll images built!"
