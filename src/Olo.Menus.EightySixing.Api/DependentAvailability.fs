module Olo.Menus.EightySixing.Api.DependentAvailability

open System
open Giraffe
open Olo.Menus.EightySixing.Core.Postgres
open Olo.Menus.EightySixing.Models
open Olo.Menus.EightySixing.Core

let getDbConnection connectionString =
    Postgres.connect connectionString

let handleGetItems (connectionString: string) (channelId: Guid) =
    use conn = getDbConnection connectionString
    let result = getDependentAvailabilityItems conn channelId
    let response = EightySixApiResponse<DependentAvailabilityItem list>(IsSuccessful=true, Errors=[], Data=result)
    Successful.OK response

let handleGetItemsForProduct (connectionString: string) (channelId: string, productId: Guid) =
    use conn = getDbConnection connectionString
    let result = getDependentAvailabilityItemsForProduct conn (Guid.Parse(channelId)) productId
    let response = EightySixApiResponse<DependentAvailabilityItem list>(IsSuccessful=true, Errors=[], Data=result)
    Successful.OK response

let handleAddItems (connectionString: string) (channelId: Guid) (items: DependentAvailabilityItem list) =
    use conn = getDbConnection connectionString
    addDependentAvailabilityItems conn channelId items
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleRemoveProductOptionGroup (connectionString: string) (channelId: string, productId: string, optionGroupId: Guid) =
    use conn = getDbConnection connectionString
    removeDependentAvailabilityItemsForProductOptionGroup conn (Guid.Parse(channelId)) (Guid.Parse(productId)) optionGroupId
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleRemoveOptionGroupChoice (connectionString: string) (channelId: string, optionGroupChoiceId: Guid) =
    use conn = getDbConnection connectionString
    removeDependentAvailabilityItemsForOptionGroupChoice conn (Guid.Parse(channelId)) optionGroupChoiceId
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleRemoveChannel (connectionString: string) (channelId: Guid) =
    use conn = getDbConnection connectionString
    removeDependentAvailabilityItems conn channelId
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleUpdateChannel (connectionString: string) (channelId: Guid) (items: DependentAvailabilityItem list) =
    use conn = getDbConnection connectionString
    updateDependentAvailabilityItems conn channelId items
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleUpdateOptionGroup (connectionString: string) (channelId: string, optionGroupId: Guid) (items: DependentAvailabilityItem list) =
    use conn = getDbConnection connectionString
    updateDependentAvailabilityItemsForOptionGroup conn (Guid.Parse(channelId)) optionGroupId items
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

let handleUpdateOptionGroupChoice (connectionString: string) (channelId: string, optionGroupChoiceId: Guid) (items: DependentAvailabilityItem list) =
    use conn = getDbConnection connectionString
    updateDependentAvailabilityItemsForOptionGroupChoice conn (Guid.Parse(channelId)) optionGroupChoiceId items
    let response = EightySixApiResponse(IsSuccessful=true, Errors=[])
    Successful.OK response

// TODO: clean up routes when Giraffe Guid fix is available
let routes (connectionString: string) = 
    choose [
        GET >=> choose [
            routeCif "/%s/products/%O" (handleGetItemsForProduct connectionString)
            routeCif "/%O" (handleGetItems connectionString)
        ]

        POST >=> choose [
            routeCif "/%O" ((handleAddItems connectionString) >> bindJson<DependentAvailabilityItem list>)
        ]

        DELETE >=> choose [
            routeCif "/%s/products/%s/optiongroups/%O" (handleRemoveProductOptionGroup connectionString)
            routeCif "/%s/optiongroupchoices/%O" (handleRemoveOptionGroupChoice connectionString)
            routeCif "/%O" (handleRemoveChannel connectionString)
        ]

        PUT >=> choose [
            routeCif "/%s/optiongroupchoices/%O" ((handleUpdateOptionGroupChoice connectionString) >> bindJson<DependentAvailabilityItem list>)
            routeCif "/%s/optiongroups/%O" ((handleUpdateOptionGroup connectionString) >> bindJson<DependentAvailabilityItem list>)
            routeCif "/%O" ((handleUpdateChannel connectionString) >> bindJson<DependentAvailabilityItem list>)
        ]

        RequestErrors.NOT_FOUND "Not Found"
    ]
