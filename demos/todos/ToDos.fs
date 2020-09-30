module Concur.Demo.ToDos

open System
open Fable.Core.JsInterop
open Fable.React
open Browser.Dom
open Concur
open Aether
open Aether.Operators

type ToDoItem =
  {
    Created : DateTime
    Title : string
    Description : string
    IsDone : bool
  }

type State =
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

module State =

  let creating : Lens<State, ToDoItem> =
    (fun x -> x.Creating), (fun v x -> { x with Creating = v })

  let toDos : Lens<State, Map<_, _>> =
    (fun x -> x.ToDos), (fun v x -> { x with ToDos = v })

  let addToDo (toDoItem : ToDoItem) state =
    {
      state with
        ToDos =
          state.ToDos
          |> Map.add (Guid.NewGuid ()) toDoItem
    }



let h1 props children = (ConcurElement.wrapReact h1) props children

let h2 props children = (ConcurElement.wrapReact h2) props children

let p props children = (ConcurElement.wrapReact p) props children

let s props children = (ConcurElement.wrapReact s) props children

let ul props children = (ConcurElement.wrapReact ul) props children

let li props children = (ConcurElement.wrapReact li) props children

let div props children = (ConcurElement.wrapReact div) props children

let form props children = (ConcurElement.wrapReact form) props children

let button props children = (ConcurElement.wrapReact button) props children

let input props = ConcurElement.wrapReact (fun injectedProps _ -> input (injectedProps @ props)) [] []

let textarea props children = (ConcurElement.wrapReact textarea) props children

let str text = ConcurElement.wrapReact (fun _ _ -> str text) [] []




let toDoEditor : ConcurApp<ToDoItem, ToDoItem> =
  (fun state ->
    form
      []
      [
        (
          input
            [
              Props.Value state.Title
            ]
          |> ConcurElement.onChange
            (fun e ->
              let title = !!e.target?value
              ConcurAction.SetState (title ^= ToDoItem.title))
        )
        (
          textarea
            [
              Props.Value state.Description
            ]
            []
          |> ConcurElement.onChange
            (fun e ->
              let description = !!e.target?value
              ConcurAction.SetState (description ^= ToDoItem.description))
        )
        (
          button [] [ str "Create" ]
          |> ConcurElement.onClick
            (fun e ->
              e.preventDefault ()

              ConcurAction.Sequence
                [
                  ConcurAction.SetState (fun _ -> ToDoItem.empty)
                  ConcurAction.Output { state with Created = DateTime.UtcNow }
                ])
        )
      ], ConcurAction.noOp)

let toDos : ConcurApp<State, Unit> =
  (fun state ->
    let toDoEditorElement, toDoEditorAction =
      (
        toDoEditor
        |> ConcurApp.transform
          State.creating
          (State.addToDo >> ConcurAction.SetState)
      ) state

    let el =
      div
        []
        [
          h1 [] [ str "ToDos" ]

          toDoEditorElement

          ul
            []
            (
              state.ToDos
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
                    p [] [ str v.Description ]
                    (
                      button [] [ if v.IsDone then str "Mark as Pending" else str "Mark as Done" ]
                      |> ConcurElement.onClick
                        (fun _ ->
                          let optic = State.toDos >-> Map.value_ k >-> Option.value_ >?> ToDoItem.isDone

                          ConcurAction.SetState (not ^% optic))
                    )
                  ]
              )
              |> Seq.toList
            )
        ]

    (el, toDoEditorAction))

let container = document.getElementById "root"

let initialState =
  {
    ToDos = Map.empty
    Creating = ToDoItem.empty
  }

Dom.runApp container toDos initialState
