module ProcessorTests

open Expecto
open NumberTrackingService.Client.Client
open NumberTrackingService.Models
open System

let tests (client:INumberTrackingServiceClient) =
    let wait seconds = Async.Sleep (seconds * 1000)
    testList "Procesor Tests" [
        testAsync "When a message is sent to the Processor, the number is saved for the correct location" {
            let expected = { LocationId = Guid.NewGuid(); Number = 42 } 
            
            do! client.SendUpdateRequestAsync expected
            do! wait 2
              
            let! actual = client.GetLocationNumberAsync expected.LocationId
            Expect.equal actual expected.Number "The number is correct"
        }
        testAsync "When messages are sent for multiple locations, the Processor handles them in the correct order" {
            let rnd = Random()
            let generateMessages locationId count =
                List.init count (fun _ -> { LocationId = locationId; Number = rnd.Next(1,1000) })

            let expectedNumber (messages: LocationNumber list) = 
                messages |> List.last |> (fun x -> x.Number)

            let sendAll = List.iter (fun x -> client.SendUpdateRequestAsync x |> Async.RunSynchronously)
                
            let location1 = Guid.NewGuid()
            let location2 = Guid.NewGuid()
            let location3 = Guid.NewGuid()

            let messages1 = generateMessages location1 10
            let messages2 = generateMessages location2 10
            let messages3 = generateMessages location3 10

            let expected1 = expectedNumber messages1
            let expected2 = expectedNumber messages2
            let expected3 = expectedNumber messages3
            
            sendAll messages1
            sendAll messages2
            sendAll messages3

            do! wait 10
              
            let! actual1 = client.GetLocationNumberAsync location1
            let! actual2 = client.GetLocationNumberAsync location2
            let! actual3 = client.GetLocationNumberAsync location3

            Expect.equal actual1 expected1 "The correct number for Location 1 was saved"
            Expect.equal actual2 expected2 "The correct number for Location 2 was saved"
            Expect.equal actual3 expected3 "The correct number for Location 3 was saved"
        }
    ]