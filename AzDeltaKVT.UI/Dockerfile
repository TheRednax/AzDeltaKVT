FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /app
COPY . .
RUN dotnet publish AzDeltaKVT.UI/AzDeltaKVT.UI.csproj -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
