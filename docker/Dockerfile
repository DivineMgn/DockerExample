FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY ./DockerExample.sln ./
COPY ./WebAPI/WebAPI.csproj ./WebAPI/
COPY ./NLog.Custom.LayoutRenderers/NLog.Custom.LayoutRenderers.csproj ./NLog.Custom.LayoutRenderers/
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /source/WebAPI
RUN dotnet publish -c release -o /source/bin

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /source/bin ./

EXPOSE 5000
ENTRYPOINT ["dotnet", "WebAPI.dll"]