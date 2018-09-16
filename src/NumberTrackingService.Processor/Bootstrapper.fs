module NumberTrackingService.Processor.Bootstrapper

open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open NumberTrackingService.Infrastructure
open NumberTrackingService.Infrastructure.Logging
open NumberTrackingService.Processor.Configuration

type ServiceHost (listenToQueue) = 
    let log = logger "ServiceHost"
    interface IHostedService with
        member x.StartAsync token = 
            log <| Info "Starting Service Host"
            listenToQueue token 
            |> Async.StartAsTask 
            |> ignore
            Task.CompletedTask

        member x.StopAsync _ = 
            log <| Info "Stopping Service Host"
            Task.CompletedTask

let createServiceHost (config:ProcessorConfiguration) = 
    let log = Logging.logger "Bootstrapper"

    log <| Info "Starting Service..."
    let sqsClient = Sqs.createClient config.LocalStackEnabled
    let saveNumber = Db.saveNumber config.ConnectionString
    
    let listenToQueue = 
        Sqs.listenToQueue 
            (Sqs.receiveMessagesAsync 
                sqsClient 
                config.LocationNumberRequestQueueUrl) 
            saveNumber

    HostBuilder().ConfigureServices(
        fun services -> 
            services.AddSingleton<IHostedService>(ServiceHost listenToQueue) 
            |> ignore)