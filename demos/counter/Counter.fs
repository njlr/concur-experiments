module Concur.Demo.Counter

open Fable.React
open Browser.Dom
open Concur

let counter : ConcurApp<int> =
  (fun count ->
    let el =
      ConcurElement.stateless
        div
        []
        [
          (ConcurElement.connected
            {
              OnClick = Some (fun _ -> [ SetState (count - 1) ])
            }
            button
            [ ConcurElement.simple (str "-1") ])
          ConcurElement.simple (str (string count))
          (ConcurElement.connected
            {
              OnClick = Some (fun _ -> [ SetState (count + 1) ])
            }
            button
            [ ConcurElement.simple (str "+1") ])
        ]

    (el, []))

let container = document.getElementById "root"

Dom.runApp container counter 0
