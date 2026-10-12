# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY WebSDD/WebSDD.csproj WebSDD/
COPY Directory.Build.props ./
RUN dotnet restore WebSDD/WebSDD.csproj
COPY . .
RUN dotnet publish WebSDD/WebSDD.csproj -c Release -o /app/publish --no-restore -p:TreatWarningsAsErrors=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebSDD.dll"]
