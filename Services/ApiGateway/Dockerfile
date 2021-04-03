FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY *.sln ./

COPY ./Libraries/PermissionLib/PermissionLib.csproj ./Libraries/PermissionLib/
COPY ./Libraries/ApiAuthLib/ApiAuthLib.csproj ./Libraries/ApiAuthLib/
COPY ./Services/ApiGateway/ApiGateway.csproj ./Services/ApiGateway/

RUN dotnet restore ./Services/ApiGateway/ApiGateway.csproj
COPY . .
WORKDIR /src/Services/ApiGateway
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
