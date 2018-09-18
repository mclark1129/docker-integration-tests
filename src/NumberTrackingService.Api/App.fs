module App

open Giraffe
open NumberTrackingService.Infrastructure
open NumberTrackingService.Models
open System

[<CLIMutable>]
type ApiConfiguration = {
    ConnectionString: string
}

let handleGetNumber (getNumber:Guid -> LocationNumber option) (locationId: Guid) =
    match getNumber locationId with 
    | Some x -> Successful.OK x.Number
    | None   -> RequestErrors.NOT_FOUND "Not Found"

let app (config: ApiConfiguration) : HttpHandler =
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true

    let getNumber = Db.getNumber config.ConnectionString

    choose [ 
        routeCif "/locations/%O" (handleGetNumber getNumber) 
        RequestErrors.NOT_FOUND "Not Found"
    ]
