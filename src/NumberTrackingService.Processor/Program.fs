open Argu
open Microsoft.Extensions.Hosting
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Processor.Bootstrapper

type ProgramArguments = Config_Source of ConfigSource
with
    interface IArgParserTemplate with
        member s.Usage = 
            match s with 
            | Config_Source _ -> "Specify the source of configuration"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<ProgramArguments>()
    let results = parser.ParseCommandLine argv
    let configSource = results.GetResult(Config_Source, App_Settings)
    
    let serviceHost = createServiceHost configSource
    serviceHost.RunConsoleAsync().Wait()

    0 // return an integer exit code