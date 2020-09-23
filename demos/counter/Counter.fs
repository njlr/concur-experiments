module Concur.Demo.Counter

open Fable.React
open Fable.React.Props
open Browser.Dom
open Concur

let rec counter : ConcurApp<int> =
  fun count ->
    (fun dispatch ->
      fragment
        []
        [
          h1
            []
            [ str (sprintf "Count: %i" count) ]
          button
            [ OnClick (fun _ -> dispatch (count - 1)) ]
            [ str "-1" ]
          button
            [ OnClick (fun _ -> dispatch (count + 1)) ]
            [ str "+1" ]
        ]),
    []

let container = document.getElementById "root"

Dom.runApp container counter 0
