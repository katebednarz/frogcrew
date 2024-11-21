cd docker
docker compose down
docker compose up -d
cd ..
sleep 10
cd backend
dotnet run