module NumberTrackingService.Client.Configuration

[<CLIMutable>]
type ClientConfiguration = {
    NumberTrackingServiceQueueUrl: string
    NumberTrackingServiceApiUrl: string
}