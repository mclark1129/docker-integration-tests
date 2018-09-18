module NumberTrackingService.Processor.Configuration

[<CLIMutable>]
type ProcessorConfiguration = {
    LocationNumberRequestQueueUrl: string
    ConnectionString: string
}