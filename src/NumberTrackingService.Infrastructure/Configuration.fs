module NumberTrackingService.Infrastructure.Configuration

open Microsoft.Extensions.Configuration

type ConfigSource = 
    | Environment_Variables
    | App_Settings

let loadConfig<'a> = function
    | Environment_Variables ->
        let typename = typeof<'a>.Name
        let config = 
            ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build()                
        config.GetSection(typename).Get<'a>()
    | App_Settings ->
        let config = 
            ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
        config.GetSection(typeof<'a>.Name).Get<'a>()