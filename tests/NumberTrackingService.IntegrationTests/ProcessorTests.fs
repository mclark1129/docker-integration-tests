module ProcessorTests

open Expecto
open NumberTrackingService.Client.Client
open NumberTrackingService.Models
open NumberTrackingService.Infrastructure.Logging
open System
open System.Threading

let tests (client:INumberTrackingServiceClient) =
    let wait () = Async.Sleep 2000
    testList "Procesor Tests" [
        testAsync "When a message is sent to the Processor, the number is saved for the correct location" {
            let log = logger "Test"
            let expected = { LocationId = Guid.NewGuid(); Number = 42 } 

            
            log <| Info "Sending Message"
            do! client.SendUpdateRequestAsync expected
            do! wait ()
              
            let! actual = client.GetLocationNumberAsync expected.LocationId
            Expect.equal actual expected.Number "The number is correct"
        }
    ]