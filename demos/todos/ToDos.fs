module Concur.Demo.ToDos

open System
open Fable.Core.JsInterop
open Fable.React
open Browser.Dom
open Concur
open Concur.Components
open Aether
open Aether.Operators

type ToDoItem =
  {
    Created : DateTime
    Title : string
    Description : string
    IsDone : bool
  }

type ToDoApp =
  {
    ToDos : Map<Guid, ToDoItem>
    Creating : ToDoItem
  }

module ToDoItem =

  let empty =
    {
      Created = DateTime.MinValue
      Title = ""
      Description = ""
      IsDone = false
    }

  let title : Lens<ToDoItem, string> =
    (fun x -> x.Title), (fun v x -> { x with Title = v })

  let description : Lens<ToDoItem, string> =
    (fun x -> x.Description), (fun v x -> { x with Description = v })

  let isDone : Lens<ToDoItem, _> =
    (fun x -> x.IsDone), (fun v x -> { x with IsDone = v })

module ToDoApp =

  let creating : Lens<ToDoApp, ToDoItem> =
    (fun x -> x.Creating), (fun v x -> { x with Creating = v })

  let toDos : Lens<ToDoApp, Map<_, _>> =
    (fun x -> x.ToDos), (fun v x -> { x with ToDos = v })

  let addToDo (toDoItem : ToDoItem) state =
    {
      state with
        ToDos =
          state.ToDos
          |> Map.add (Guid.NewGuid ()) toDoItem
    }

let toDoEditor : ConcurApp<ToDoItem, ToDoItem> =
  (fun state ->
    form
      []
      [
        input
          [
            Props.Value state.Title
          ]
        |> ConcurElement.onChange
          (fun e ->
            let title = !!e.target?value
            ConcurAction.SetState (title ^= ToDoItem.title))

        textarea
          [
            Props.Value state.Description
          ]
          []
        |> ConcurElement.onChange
          (fun e ->
            let description = !!e.target?value
            ConcurAction.SetState (description ^= ToDoItem.description))

        button [] [ str "Create" ]
        |> ConcurElement.onClick
          (fun e ->
            e.preventDefault ()

            ConcurAction.Sequence
              [
                ConcurAction.SetState (fun _ -> ToDoItem.empty)
                ConcurAction.Output { state with Created = DateTime.UtcNow }
              ])
      ])

let toDos : ConcurApp<Map<Guid, ToDoItem>, Unit> =
  (fun state ->
    (ul
      []
      (
        state
        |> Map.toSeq
        |> Seq.sortByDescending (fun (k, v) -> v.Created)
        |> Seq.map (fun (k, v) ->
          li
            [
              Props.Prop.Key (string k)
            ]
            [
              h2 [] [ if v.IsDone then s [] [ str v.Title ] else str v.Title ]
              p [] [ str (string v.Created) ]
              p [] [ if v.IsDone then s [] [ str v.Description ] else str v.Description ]
              (
                button [] [ if v.IsDone then str "Mark as Pending" else str "Mark as Done" ]
                |> ConcurElement.onClick
                  (fun _ ->
                    let optic = Map.value_ k >-> Option.value_ >?> ToDoItem.isDone

                    ConcurAction.SetState (not ^% optic))
              )
            ]
        )
        |> Seq.toList
      )))

let app : ConcurApp<ToDoApp, Unit> =
  (fun state ->
      div
        []
        [
          h1 [] [ str "ToDos" ]

          toDoEditor
          |> ConcurApp.transform
            ToDoApp.creating
            (ToDoApp.addToDo >> ConcurAction.SetState)
          |> ConcurApp.toElement state

          toDos
          |> ConcurApp.transform
            ToDoApp.toDos
            (fun _ -> ConcurAction.noOp)
          |> ConcurApp.toElement state
        ])

let container = document.getElementById "root"

let initialState =
  {
    ToDos = Map.empty
    Creating = ToDoItem.empty
  }

Dom.runApp container app initialState
