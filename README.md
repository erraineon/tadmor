# tadmor
1. install .net core 3.0
2. add `https://www.myget.org/F/discord-net/api/v3/index.json` to your nuget sources
3. install the Entity Framework Core tools with `dotnet tool install --global dotnet-ef`
4. build `Tadmor.csproj`
5. rename `appsettings.sample.json` in the output folder to `appsettings.json`
6. add your bot token to it and your other settings

`.help` for a list of commands

`.tf` and `.swap` work only on Windows and require latest visual c++ redistributable
