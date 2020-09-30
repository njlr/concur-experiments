namespace Concur

open Fable
open Fable.React
open Fable.SimpleHttp
open Browser.Types
open Aether
open Aether.Operators

type ConcurAction<'tstate, 'toutput> =
  | SetState of ('tstate -> 'tstate)
  | Fetch of HttpRequest * (HttpResponse -> ConcurAction<'tstate, 'toutput>)
  | ConsoleLog of string
  | Output of 'toutput
  | Sequence of ConcurAction<'tstate, 'toutput> list

type Connections<'tstate, 'toutput> =
  {
    OnClick : (MouseEvent -> ConcurAction<'tstate, 'toutput>) option
    OnChange : (Event -> ConcurAction<'tstate, 'toutput>) option
  }

type ConcurElement<'tstate, 'toutput> =
  {

    Connections : Connections<'tstate, 'toutput>
    RenderReact : (Props.IHTMLProp list -> ReactElement list -> ReactElement)
    Children : ConcurElement<'tstate, 'toutput> list
  }

type ConcurApp<'tstate, 'toutput> = 'tstate -> ConcurElement<'tstate, 'toutput> * ConcurAction<'tstate, 'toutput>

module ConcurAction =

  let noOp = Sequence []

  let transform (lens : Lens<'tnewstate, 'tstate>) (dispatch : 'toutput -> ConcurAction<'tnewstate, 'tnewoutput>) (action : ConcurAction<'tstate, 'toutput>) : ConcurAction<'tnewstate, 'tnewoutput> =
    let rec loop action =
      match action with
      | SetState (update : 'tstate -> 'tstate) -> SetState (update ^% lens)
      | Fetch (req, callback) -> Fetch (req, callback >> loop)
      | Output x -> dispatch x
      | ConsoleLog x -> ConsoleLog x
      | Sequence xs -> Sequence (xs |> List.map loop)

    loop action

  let merge (first : ConcurAction<_, _>) (second : ConcurAction<_, _>) =
    match first, second with
    | Sequence xs, Sequence ys -> Sequence (xs @ ys)
    | Sequence xs, _ -> Sequence (xs @ [ second ])
    | _, Sequence ys -> Sequence (first :: ys)
    | _, _ -> Sequence [ first; second ]

  let mergeAll (actions : seq<ConcurAction<_, _>>) =
    actions
    |> Seq.fold merge (Sequence [])

module Connections =

  let zero () =
    {
      OnClick = None
      OnChange = None
    }

  let onClick handler =
    {
      zero () with
        OnClick = Some handler
    }

module ConcurElement =

  let create connections reactElementFn children =
    {
      Connections = connections
      RenderReact = reactElementFn
      Children = children
    }

  let wrapReact reactElementFn props children =
    create
      (Connections.zero ())
      (fun reactProps reactChildren -> reactElementFn (props @ reactProps) reactChildren)
      children

  let rec transform (lens : Lens<'tnewstate, 'tstate>) (dispatch : 'toutput -> ConcurAction<'tnewstate, 'tnewoutput>) (element : ConcurElement<'tstate, 'toutput>) : ConcurElement<'tnewstate, 'tnewoutput> =
    {
      Connections =
        {
          OnClick =
            element.Connections.OnClick
            |> Option.map
              (fun handler ->
                (fun e ->
                  let action = handler e
                  action |> ConcurAction.transform lens dispatch))
          OnChange =
            element.Connections.OnChange
            |> Option.map
              (fun handler ->
                (fun e ->
                  let action = handler e
                  action |> ConcurAction.transform lens dispatch))
        }
      RenderReact = element.RenderReact
      Children =
        element.Children
        |> List.map (transform lens dispatch)
    }

  let onClick handler element =
    {
      element with
        Connections =
          {
            element.Connections with
              OnClick = Some handler
          }
    }

  let onChange handler element =
    {
      element with
        Connections =
          {
            element.Connections with
              OnChange = Some handler
          }
    }

module ConcurApp =

  let transform (lens : Lens<'tnewstate, 'tstate>) (dispatch : 'toutput -> ConcurAction<'tnewstate, 'tnewoutput>) (app : ConcurApp<'tstate, 'toutput>) : ConcurApp<'tnewstate, 'tnewoutput> =
    (fun (newState : 'tnewstate) ->
      let state = newState ^. lens
      let element, action = app state

      let element = ConcurElement.transform lens dispatch element
      let action = ConcurAction.transform lens dispatch action

      element, action)
