rm -r -force src/NumberTrackingService.Api/publish

dotnet build -c Release
dotnet publish src/NumberTrackingService.Processor -c Release -o publish --no-build
dotnet publish src/NumberTrackingService.Api -c Release -o publish --no-build
dotnet publish tests/NumberTrackingService.IntegrationTests -c Release -o publish --no-build