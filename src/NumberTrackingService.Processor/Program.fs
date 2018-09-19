open Argu
open Microsoft.Extensions.Hosting
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Processor.Bootstrapper
open System.Threading

type Arguments = 
    | Config_Source of ConfigSource
    | Use_LocalStack of bool 
    | Local_Sqs_Url of string option
    | Startup_Delay of int
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with 
            | Config_Source _  -> "Specify the source of configuration"
            | Use_LocalStack _ -> "The infrastructure backend"
            | Local_Sqs_Url  _ -> "If using LocalStack, what is the SQS URL"
            | Startup_Delay  _ -> "How long to wait before starting the service"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.ParseCommandLine argv
    let configSource = results.GetResult(Config_Source, App_Settings)
    let useLocalStack = results.GetResult(Use_LocalStack, true)
    let localSqsUrl = results.GetResult(Local_Sqs_Url, None)
    let startupDelay = results.GetResult(Startup_Delay, 0)
    let serviceHost = createServiceHost configSource (useLocalStack, localSqsUrl)

    Thread.Sleep (startupDelay * 1000)
    serviceHost.RunConsoleAsync().Wait()

    0 // return an integer exit code