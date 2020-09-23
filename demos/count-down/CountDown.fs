module Concur.Demo.CountDown

open Fable.React
open Fable.React.Props
open Browser.Dom
open Concur

let rec countDown : ConcurApp<int> =
  (fun count ->
    (fun dispatch ->
      if count = 0
      then
        fragment
          []
          [
            h1
              []
              [ str "Blast Off!" ]
            button
              [ OnClick (fun _ -> dispatch 5) ]
              [ str "Restart" ]
          ]
      else
        h1
          []
          [ str (string count) ]
          ),
      [
        if count > 0 then Sleep (1_000, fun count -> count - 1) else NoOp
      ])

let container = document.getElementById "root"

Dom.runApp container countDown 5
