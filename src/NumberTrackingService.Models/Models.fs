module NumberTrackingService.Models

open System

[<CLIMutable>]
type LocationNumber = { LocationId: Guid; Number: int }