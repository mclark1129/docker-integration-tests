open Argu
open Microsoft.Extensions.Hosting
open NumberTrackingService.Infrastructure
open NumberTrackingService.Infrastructure.Logging
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Processor.Bootstrapper
open System.Threading

type Arguments = 
    | Config_Source of ConfigSource
    | Use_LocalStack of bool 
    | Local_Sqs_Url of string option
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with 
            | Config_Source _  -> "Specify the source of configuration"
            | Use_LocalStack _ -> "The infrastructure backend"
            | Local_Sqs_Url  _ -> "Blah"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.ParseCommandLine argv
    let configSource = results.GetResult(Config_Source, App_Settings)
    let useLocalStack = results.GetResult(Use_LocalStack, true)
    let localSqsUrl = results.GetResult(Local_Sqs_Url, None)
    let serviceHost = createServiceHost configSource (useLocalStack, localSqsUrl)

    let log = logger "Startup"
    serviceHost.RunConsoleAsync().Wait()

    0 // return an integer exit code