# Build runtime image
FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY publish/ .
ENTRYPOINT [ "dotnet", "NumberTrackingService.IntegrationTests.dll", "--config-source", "environment-variables", "--use-localstack", "true", "--local-sqs-url", "http://localstack:4576", "--startup-delay", "15"]