module NumberTrackingService.Infrastructure.Sqs

open Amazon.SQS
open Amazon.SQS.Model
open System.Threading
open Newtonsoft.Json
open NumberTrackingService.Infrastructure.Logging
open System.Net.Http
open System

type QueueMessage<'a> = { MessageId: string; ReceiptHandle: string; Body: 'a }

let receiveMessagesAsync<'a> (sqsClient:IAmazonSQS) queueUrl () = async {
    let log = logger "Receive"
    log <| Info (sprintf "%s" sqsClient.Config.ServiceURL)
    log <| Info (sprintf "%s" queueUrl)

    let req = ReceiveMessageRequest queueUrl
    let! rsp = sqsClient.ReceiveMessageAsync req |> Async.AwaitTask

    return rsp.Messages 
    |> Seq.map (fun x -> { 
        MessageId = x.MessageId
        ReceiptHandle = x.ReceiptHandle
        Body = JsonConvert.DeserializeObject<'a> x.Body 
    })
}

let deleteMessageAsync (sqsClient:IAmazonSQS) queueUrl receiptHandle =
    DeleteMessageRequest(queueUrl, receiptHandle)
    |> sqsClient.DeleteMessageAsync
    |> Async.AwaitTask 
    |> Async.Ignore

let enqueueAsync<'a> (sqsClient:IAmazonSQS) queueUrl (message:'a) =
    SendMessageRequest(queueUrl, JsonConvert.SerializeObject message)
    |> sqsClient.SendMessageAsync 
    |> Async.AwaitTask 
    |> Async.Ignore
    
let enqueueFifoAsync<'a> (sqsClient:IAmazonSQS) queueUrl (message:'a) messageGroupId messageDeduplicationId =
    SendMessageRequest(queueUrl, JsonConvert.SerializeObject message, MessageGroupId = "abc", MessageDeduplicationId = "def")
    |> sqsClient.SendMessageAsync 
    |> Async.AwaitTask 
    |> Async.Ignore

let createClient backend =
    let config = 
        match backend with
        | (true, Some url) -> AmazonSQSConfig (ServiceURL = url)
        | (true, None)     -> AmazonSQSConfig (ServiceURL = "http://localhost:4576")
        | _                -> AmazonSQSConfig ()
    
    new AmazonSQSClient(config)

type SqsClient (queueUrl, infrastructure) = 
    let log = logger "SqsClient"
    let _sqsClient = createClient infrastructure

    member x.ListenToQueueAsync<'a> handleMessage (ct:CancellationToken) = 
        let receive = receiveMessagesAsync<'a> _sqsClient queueUrl
        let delete = deleteMessageAsync _sqsClient queueUrl

        let rec loop () = async {
            if ct.IsCancellationRequested then return ()

            log <| Debug "Receiving Messages"
            let! messages = receive ()
            messages |> Seq.iter (fun msg -> 
                handleMessage msg.Body 
                delete msg.ReceiptHandle |> Async.RunSynchronously)
                
            return! loop ()
        }

        loop ()
    
    member x.EnqueueAsync<'a> message = enqueueAsync<'a> _sqsClient queueUrl message
    member x.EnqueueFifoAsync<'a> message messageGroupId deduplicationId = 
        enqueueFifoAsync<'a> 
            _sqsClient 
            queueUrl 
            message 
            messageGroupId 
            deduplicationId
