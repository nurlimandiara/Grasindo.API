FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["Grasindo.API.csproj", "./"]
COPY . .
WORKDIR "/src/."
RUN sh -c "dotnet restore -s ./packages \
                -s https://api.nuget.org/v3/index.json \
                ./Grasindo.API.csproj && \
            dotnet build Grasindo.API.csproj -c Release -o /app/build && \
            rm -R ./packages"

FROM build AS publish
RUN dotnet publish "Grasindo.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Grasindo.API.dll"]
