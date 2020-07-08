FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["Grasindo.API.csproj", "./"]
RUN dotnet restore "./Grasindo.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Grasindo.API.csproj" -c Release -o /app/build --runtime alpine-x64

FROM build AS publish
RUN dotnet publish "Grasindo.API.csproj" -c Release -o /app/publish --runtime alpine-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Grasindo.API.dll"]
