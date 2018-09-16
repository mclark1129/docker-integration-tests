module Olo.Menus.EightySixing.Api.Program
module App = Olo.Menus.EightySixing.Api.App

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Cors.Infrastructure
open NLog.Web
open Newtonsoft.Json
open Argu
open Giraffe
open Olo.Menus.EightySixing.Models
open Olo.Menus.EightySixing.Core.Configuration
open Olo.Menus.EightySixing.Api.Configuration

type Arguments =
    | Config_Source of ConfigSource
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with
            | Config_Source _ -> "Specify the source of the configuration."

let configureServices (services : IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.ClearProviders() |> ignore
    builder.SetMinimumLevel Microsoft.Extensions.Logging.LogLevel.Trace |> ignore

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader() |> ignore

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError (EventId(), ex, ex.Message)
    match ex with
    | :? JsonSerializationException -> 
        let response = EightySixApiResponse(IsSuccessful=true, Errors=[ ex.Message ])
        clearResponse >=> setStatusCode 400 >=> json response
    | _ ->
        let response = EightySixApiResponse(IsSuccessful=true, Errors=[ "An unhandled exception has occurred while executing the request." ])
        clearResponse >=> setStatusCode 500 >=> json response
    
let configureApp (configSource: ConfigSource) (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    let config = loadConfig<ApiConfig> configSource

    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler(errorHandler))
       .UseCors(configureCors)
       .UseGiraffe(App.app <| config)    

[<EntryPoint>]
let main args = 
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.Parse args
    let configSource = results.GetResult(Config_Source, defaultValue = Config_Cache)
    printfn "Config Source: %s" <| configSource.ToString()
    
    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .ConfigureServices(configureServices)
        .Configure(Action<IApplicationBuilder> (configureApp configSource))
        .ConfigureLogging(configureLogging)
        .UseNLog()
        .Build()
        .Run()
    0    