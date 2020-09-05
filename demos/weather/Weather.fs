module Concur.Demo.Weather

open System
open FSharp.Control
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Browser.Dom
open Concur
open Thoth.Json

type ConsolidatedWeather =
  {
    Id : uint64
    Created : DateTime
    ApplicableDate : DateTime
    WeatherStateName : string
    MinTemp : float
    MaxTemp : float
    Humidity : int
  }

type Weather =
  {
    Title : string
    LocationType : string
    ConsolidatedWeather : ConsolidatedWeather list
  }

module Result =

  let toOption =
    function
    | Ok x -> Some x
    | Error _ -> None

module Decode =

  let consolidatedWeather =
    Decode.object
      (fun get ->
        {
          Id = get.Required.Field "id" Decode.uint64
          Created = get.Required.Field "created" Decode.datetime
          ApplicableDate = get.Required.Field "applicable_date" Decode.datetime
          WeatherStateName = get.Required.Field "weather_state_name" Decode.string
          MinTemp = get.Required.Field "min_temp" Decode.float
          MaxTemp = get.Required.Field "max_temp" Decode.float
          Humidity = get.Required.Field "humidity" Decode.int
        })

  let weather =
    Decode.object
      (fun get ->
        {
          Title = get.Required.Field "title" Decode.string
          LocationType = get.Required.Field "location_type" Decode.string
          ConsolidatedWeather = get.Required.Field "consolidated_weather" (Decode.list consolidatedWeather)
        })

let private truncate n (x : string) =
  if x.Length > n
  then
    x.Substring (0, n - 3) + "..."
  else
    x

type FetchError =
  | DecodingError of string
  | HttpError of int * string

let fetchWeather =
  async {
    let! status, text = Http.get "/api/location/44418/"

    return
      match status with
      | 200 ->
        Decode.fromString Decode.weather text
        |> Result.mapError DecodingError
      | _ ->
        Error (HttpError (status, text))
  }

let private loading : ConcurApp =
  asyncSeq {
    yield p [] [ str "Loading... " ]
  }

let private failure (error : FetchError) : ConcurApp =
  asyncSeq {
    match error with
    | DecodingError error ->
      yield
        fragment
          []
          [
            h2 [] [ str "Decoding Error" ]
            p [] [ str (truncate 200 error) ]
          ]
    | HttpError (status, text) ->
      yield
        fragment
          []
          [
            h2 [] [ str (string status) ]
            p [] [ str (truncate 200 text) ]
          ]
  }

let private success (weather : Weather) : ConcurApp =
  asyncSeq {
    yield
      ul
        []
        (
          weather.ConsolidatedWeather
          |> Seq.map (fun x ->
            li
              []
              [
                h3 [] [ str (x.ApplicableDate.ToString "yyyy-MM-dd") ]
                p [] [ str x.WeatherStateName ]

                dl
                  []
                  [
                    dt [] [ str "Temperature" ]
                    dd [] [ str (sprintf "%.2f℃ - %.2f℃" x.MinTemp x.MaxTemp) ]

                    dt [] [ str "Humidity" ]
                    dd [] [ str (sprintf "%i%%" x.Humidity) ]
                  ]
              ]
          )
          |> Seq.toList
        )
  }

let rec weather lastItem : ConcurApp =
  asyncSeq {
    match lastItem with
    | Some weather ->
      yield! success weather
    | None ->
      yield! loading

    let! response = fetchWeather

    match response with
    | Ok weather ->
      yield! success weather

      do! Async.Sleep 30_000
    | Error error ->
      use w = new Waiter<_> ()

      yield!
        failure error
        |> AsyncSeq.map (fun x ->
          fragment
            []
            [
              x
              button
                [
                  OnClick (fun _ -> w.Resolve ())
                ]
                [ str "Refresh" ]
            ]
        )

      // Wait for user input
      do! w.Resolved

    // Recurse!
    yield! weather (Result.toOption response)
  }

let container = document.getElementById "root"

Dom.runApp container (weather None)
