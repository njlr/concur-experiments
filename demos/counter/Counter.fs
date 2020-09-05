module Concur.Demo.Counter

open FSharp.Control
open Fable.React
open Fable.React.Props
open Browser.Dom
open Concur

let rec counter (count : int) : ConcurApp =
  asyncSeq {
    use w = new Waiter<_> ()

    // Yield the "view" to the controller
    yield
      fragment
        []
        [
          h1
            []
            [ str (sprintf "Count: %i" count) ]
          button
            [ OnClick (fun _ -> w.Resolve (count - 1)) ]
            [ str "-1" ]
          button
            [ OnClick (fun _ -> w.Resolve (count + 1)) ]
            [ str "+1" ]
        ]

    // Wait for user input
    let! next = w.Resolved

    // Recurse!
    yield! counter next
  }

let container = document.getElementById "root"

Dom.runApp container (counter 0)
