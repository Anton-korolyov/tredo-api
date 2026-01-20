# =============================
# BUILD
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and restore
COPY Tredo.Api.sln .
COPY Tredo.Api/Tredo.Api.csproj Tredo.Api/

RUN dotnet restore Tredo.Api.sln

# copy everything and publish
COPY . .
RUN dotnet publish Tredo.Api/Tredo.Api.csproj -c Release -o /app/publish

# =============================
# RUNTIME
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Tredo.Api.dll"]
