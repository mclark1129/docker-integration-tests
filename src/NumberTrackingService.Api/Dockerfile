# Build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY publish/ .
EXPOSE 80

ENTRYPOINT ["dotnet", "NumberTrackingService.Api.dll", "--config-source", "environment-variables"]