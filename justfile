build:
    dotnet build Identisure.Client.slnx

test:
    dotnet test Identisure.Client.slnx

check:
    dotnet format Identisure.Client.slnx --verify-no-changes
    dotnet build Identisure.Client.slnx --no-restore /warnaserror

publish:
    dotnet pack src/Identisure.Client/Identisure.Client.csproj -c Release
