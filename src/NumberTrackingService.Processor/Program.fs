open Argu
open Microsoft.Extensions.Hosting
open System.Threading.Tasks

open NumberTrackingService.Infrastructure.Configuration

type ProgramArguments = Config_Source of ConfigSource
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with 
            | Config_Source _ -> "Specify the source of configuration"

type ServiceHost (listenToQueue) = 
    interface IHostedService with
        member x.StartAsync token = listenToQueue token |> Async.StartAsTask :> Task
        member x.StopAsync _ = Task.CompletedTask

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<ProgramArguments>()
    let results = parser.ParseCommandLine argv
    0 // return an integer exit code