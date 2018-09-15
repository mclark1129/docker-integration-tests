module NumberTrackingService.Infrastructure.Sqs

open Amazon.SQS
open Amazon.SQS.Model
open System.Threading
open Newtonsoft.Json

let receiveMessagesAsync<'a> (sqsClient:IAmazonSQS) queueUrl () = async {
    let req = ReceiveMessageRequest queueUrl
    let! rsp = sqsClient.ReceiveMessageAsync req |> Async.AwaitTask
    return rsp.Messages |> Seq.map (fun x -> JsonConvert.DeserializeObject<'a> x.Body)
}

let sendMessageAsync (sqsClient:IAmazonSQS) queueUrl message =
    SendMessageRequest(queueUrl, JsonConvert.SerializeObject message)
    |> sqsClient.SendMessageAsync 
    |> Async.AwaitTask 
    |> Async.Ignore

let listenToQueue receiveMessagesAsync handleMessage (cancellationToken:CancellationToken) =
    let rec loop () = async {
        let! messages = receiveMessagesAsync ()
        messages |> Seq.iter handleMessage

        if cancellationToken.IsCancellationRequested 
        then return ()
        else return! loop ()
    }

    loop ()