namespace NumberTrackingService.Client.Client

open NumberTrackingService.Models
open NumberTrackingService.Client.Configuration
open NumberTrackingService.Infrastructure
open NumberTrackingService.Infrastructure.Logging
open System
open System.Net.Http

type INumberTrackingServiceClient = 
    abstract member SendUpdateRequestAsync : LocationNumber -> Async<unit>
    abstract member GetLocationNumberAsync : Guid -> Async<int>

type NumberTrackingServiceClient (config: ClientConfiguration, backend) =
    let log = logger "Client"
    let _sqsClient = Sqs.SqsClient(config.NumberTrackingServiceQueueUrl, backend)
    let _httpClient = new HttpClient(BaseAddress = Uri config.NumberTrackingServiceApiUrl)
    interface INumberTrackingServiceClient with
        member x.SendUpdateRequestAsync msg = 
            log <| Info (sprintf "Enqueuing LocationId: %A, Number: %i" msg.LocationId msg.Number) 
            _sqsClient.EnqueueFifoAsync msg 
                (msg.LocationId.ToString())
                (Guid.NewGuid().ToString())

        member x.GetLocationNumberAsync locationId = async {
            let! response = _httpClient.GetAsync (sprintf "locations/%A" locationId) |> Async.AwaitTask
            
            if response.IsSuccessStatusCode
            then 
                let! x = response.Content.ReadAsStringAsync () |> Async.AwaitTask
                return x |> int
            else return -1
        }
            