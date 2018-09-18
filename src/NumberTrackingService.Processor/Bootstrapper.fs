module NumberTrackingService.Processor.Bootstrapper

open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open NumberTrackingService.Models
open NumberTrackingService.Infrastructure
open NumberTrackingService.Infrastructure.Configuration
open NumberTrackingService.Infrastructure.Logging
open NumberTrackingService.Processor.Configuration

type ServiceHost (listenToQueue) = 
    let log = logger "ServiceHost"

    let cts = new CancellationTokenSource()
    interface IHostedService with
        member x.StartAsync _ = 
            log <| Info "Starting Service Host"
            listenToQueue cts.Token 
            |> Async.StartAsTask 
            |> ignore
            Task.CompletedTask

        member x.StopAsync _ = 
            log <| Info "Stopping Service Host"
            cts.Cancel()
            cts.Dispose()
            Task.CompletedTask
       
let messageHandler saveNumber (message:LocationNumber) = 
    let log = logger "Message Handler"
    log <| Info (sprintf "Message Received for Location %A" message.LocationId)

    saveNumber message

let createServiceHost source backend = 
    let log = logger "Bootstrapper"

    log <| Info "Starting Service..."
    let config = loadConfig<ProcessorConfiguration> source

    let saveNumber = Db.saveNumber config.ConnectionString
    let handler = messageHandler saveNumber

    let client = Sqs.SqsClient(config.LocationNumberRequestQueueUrl, backend)

    HostBuilder().ConfigureServices(
        fun services -> 
            services.AddSingleton<IHostedService>(ServiceHost <| client.ListenToQueueAsync handler)
            |> ignore)