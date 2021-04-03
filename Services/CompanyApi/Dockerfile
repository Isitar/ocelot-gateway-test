FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ./Libraries/PermissionLib/PermissionLib.csproj ./Libraries/PermissionLib/
COPY ./Libraries/ApiAuthLib/ApiAuthLib.csproj ./Libraries/ApiAuthLib/
COPY ["Services/CompanyApi/CompanyApi.csproj", "Services/CompanyApi/"]
RUN dotnet restore "Services/CompanyApi/CompanyApi.csproj"
COPY . .
WORKDIR "/src/Services/CompanyApi"
RUN dotnet build "CompanyApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CompanyApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompanyApi.dll"]
