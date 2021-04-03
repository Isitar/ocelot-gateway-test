param(
[Parameter(Position=0)]
[string]$name)
dotnet ef migrations add $name -o Identity/Migrations --startup-project ../Api/Api.csproj --context AppIdentityDbContext
