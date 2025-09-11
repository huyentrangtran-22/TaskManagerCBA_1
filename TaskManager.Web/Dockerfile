# Dùng .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ code
COPY . .

# Restore + Publish
RUN dotnet restore TaskManager.Web/TaskManager.Web.csproj
RUN dotnet publish TaskManager.Web/TaskManager.Web.csproj -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Render sẽ tự inject $PORT
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "TaskManager.Web.dll"]
