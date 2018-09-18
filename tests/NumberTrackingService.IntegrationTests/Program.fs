// Learn more about F# at http://fsharp.org

open Expecto
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Client.Configuration
open NumberTrackingService.Client.Client

[<EntryPoint>]
let main argv =

    let config = loadConfig<ClientConfiguration> App_Settings
    let client = NumberTrackingServiceClient config
    Tests.runTests defaultConfig <| Tests.testSequenced (ProcessorTests.tests client)