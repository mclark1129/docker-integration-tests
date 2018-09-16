module Olo.Menus.EightySixing.Api.App

open Giraffe
open Olo.Menus.EightySixing.Api.Configuration

let app (config: ApiConfig) : HttpHandler = 
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true

    choose [        
        subRouteCi "/dependentavailability" <| DependentAvailability.routes config.ConnectionString
        routeCi "/healthcheck" >=> text "OK"

        RequestErrors.NOT_FOUND "Not Found"
    ]