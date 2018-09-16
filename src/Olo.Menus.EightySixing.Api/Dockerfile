FROM microsoft/aspnetcore:2.0
WORKDIR /app

COPY ./publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "Olo.Menus.EightySixing.Api.dll", "--config-source", "environment-variables"]