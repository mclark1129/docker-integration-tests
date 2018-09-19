// Learn more about F# at http://fsharp.org

open Expecto
open NumberTrackingService.Infrastructure
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Infrastructure.Logging
open NumberTrackingService.Client.Configuration
open NumberTrackingService.Client.Client
open Argu
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
            | Config_Source  _  -> "Specify the source of configuration"
            | Use_LocalStack _  -> "The SQS backend to use"
            | Local_Sqs_Url  _  -> "The url of the local SQS service"
            | Startup_Delay  _  -> "How long in seconds to wait before running tests"

[<EntryPoint>]
let main argv =
    let log = logger "Test Startup"
    let parser = ArgumentParser.Create<Arguments>()
    let results = parser.ParseCommandLine argv

    let configSource = results.GetResult(Config_Source, App_Settings)
    let useLocalStack = results.GetResult(Use_LocalStack, true)
    let localSqsUrl   = results.GetResult(Local_Sqs_Url, None) 
    let startupDelay = results.GetResult(Startup_Delay, 0)

    let config = loadConfig<ClientConfiguration> configSource
    log <| Debug (sprintf "Loading Configuration (%O)" configSource)

    log <| Debug "Waiting to Start"
    Thread.Sleep (startupDelay * 1000)
    let client = NumberTrackingServiceClient(config, (useLocalStack, localSqsUrl))
    Tests.runTests defaultConfig <| Tests.testSequenced (ProcessorTests.tests client)