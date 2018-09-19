module NumberTrackingService.Infrastructure.Sqs

open System.Threading
open Amazon.SQS
open Amazon.SQS.Model
open Newtonsoft.Json
open NumberTrackingService.Infrastructure.Logging

type QueueMessage<'a> = { MessageId: string; ReceiptHandle: string; Body: 'a }

let receiveMessagesAsync<'a> (sqsClient:IAmazonSQS) queueUrl () = async {
    let log = logger "SQS Receive"
    try 
        let req = ReceiveMessageRequest(queueUrl, MaxNumberOfMessages=1, WaitTimeSeconds=10)

        log <| Info "Receiving Messages"
        let! rsp = sqsClient.ReceiveMessageAsync req |> Async.AwaitTask

        return rsp.Messages 
        |> Seq.map (fun x -> { 
            MessageId = x.MessageId
            ReceiptHandle = x.ReceiptHandle
            Body = JsonConvert.DeserializeObject<'a> x.Body 
        })
    with 
    | ex -> 
        log <| Error (sprintf "%s" (ex.ToString()))
        return Seq.empty
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
    let log = logger "Enqueue"
    try 
        SendMessageRequest(queueUrl, JsonConvert.SerializeObject message, MessageGroupId = messageGroupId, MessageDeduplicationId = messageDeduplicationId)
        |> sqsClient.SendMessageAsync 
        |> Async.AwaitTask 
        |> Async.Ignore
    with
    | ex -> 
        async { return log <| Error (sprintf "%s" (ex.ToString())) }

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
