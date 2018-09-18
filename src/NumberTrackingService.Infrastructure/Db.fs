module NumberTrackingService.Infrastructure.Db

open System
open System.Data
open Dapper
open Npgsql
open NumberTrackingService.Models

let connect connectionString = 
    let conn = new NpgsqlConnection(connectionString) 
    conn.Open()
    conn :> IDbConnection

let saveNumber connectionString (number:LocationNumber) =
    use conn = connect connectionString
    let sql = @"INSERT INTO location_numbers (location_id, number) VALUES (@locationId, @number)
               ON CONFLICT (location_id) DO UPDATE SET number = EXCLUDED.number"
    
    conn.Execute(sql = sql, param = number) |> ignore

let getNumber connectionString (locationId:Guid) = 
    use conn = connect connectionString

    let sql = "SELECT location_id, number FROM location_numbers WHERE location_id = @locationId"
    let param = new DynamicParameters()
    param.Add("locationId", locationId, Nullable DbType.Guid)

    let number = conn.QueryFirstOrDefault<LocationNumber>(sql, param = param) 
    match box number with
    | null -> None
    | _    -> Some number