module Concur.Demo.Counter

open Browser.Dom
open Concur
open Concur.Components

let counter : ConcurApp<int, Unit> =
  (fun count ->
    div
      []
      [
        button [] [ str "-1" ]
        |> ConcurElement.onClick (fun _ -> SetState (fun x -> x - 1))

        str (string count)

        button [] [ str "+1" ]
        |> ConcurElement.onClick (fun _ -> SetState (fun x -> x + 1))
      ])

let container = document.getElementById "root"

Dom.runApp container counter 0
