module NumberTrackingService.Api.Program

open System
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open NumberTrackingService.Infrastructure.Configuration
open Microsoft.AspNetCore.Builder
open Argu
open Microsoft.AspNetCore.Cors.Infrastructure

type Arguments =
    | Config_Source of ConfigSource
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | Config_Source _ -> "Specify the source of the configuration."

let configureCors (builder: CorsPolicyBuilder) = 
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader() |> ignore

let configureServices (services : IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureApplication (configSource: ConfigSource) (app: IApplicationBuilder) = 
    let config = loadConfig<App.ApiConfiguration> configSource
    app.UseCors(configureCors).UseGiraffe(App.app config)

[<EntryPoint>]
let main args =
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.Parse args
    let configSource = results.GetResult(Config_Source, defaultValue = App_Settings)

    WebHostBuilder()
        .UseKestrel()
        .ConfigureServices(configureServices)
        .Configure(Action<IApplicationBuilder> (configureApplication configSource))
        .Build()
        .Run()

    0
