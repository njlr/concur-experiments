module Concur.Demo.Counter

open Fable.React
open Browser.Dom
open Concur

let div = ConcurElement.wrapReact div

let str text = ConcurElement.wrapReact (fun _ _ -> str text) [] []

let counter : ConcurApp<int, Unit> =
  (fun count ->
    let el =
      div
        []
        [
          (ConcurElement.create
            (Connections.onClick (fun _ -> [ SetState (fun x -> x - 1) ]))
            button
            [ str "-1" ])
          (str (string count))
          (ConcurElement.create
            (Connections.onClick (fun _ -> [ SetState (fun x -> x + 1) ]))
            button
            [ str "+1" ])
        ]

    (el, []))

let container = document.getElementById "root"

Dom.runApp container counter 0
