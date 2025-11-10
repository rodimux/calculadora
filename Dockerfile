FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el repo y publicamos solo la API
COPY . .
RUN dotnet publish src/CalculadoraCostes.Api/CalculadoraCostes.Api.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render expone 10000 por defecto, pero ASP.NET usa ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CalculadoraCostes.Api.dll"]
