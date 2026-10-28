build:
    dotnet build Identisure.Client.sln

test:
    dotnet test Identisure.Client.sln

check:
    dotnet format Identisure.Client.sln --verify-no-changes
    dotnet build Identisure.Client.sln --no-restore /warnaserror

publish:
    dotnet pack src/Identisure.Client/Identisure.Client.csproj -c Release

gen-doc:
    dotnet tool restore
    dotnet docfx docfx.json
