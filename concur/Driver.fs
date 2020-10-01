module Concur.Driver

open System.Threading
open FSharp.Control
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Fable.React
open Concur

let rec private collectRenderActions (state) (concurElement : ConcurElement<_, _>) =
  seq {
    match concurElement.Connections.OnRender with
    | Some onRender -> yield onRender state
    | None -> ()

    yield!
      concurElement.Children
      |> Seq.collect (collectRenderActions state)
  }

let rec private renderConcurElement (dispatch) (concurElement : ConcurElement<_, _>) =
  let connections = concurElement.Connections
  let children = concurElement.Children
  let reactElementFn = concurElement.RenderReact

  let reactProps =
    seq {
      match connections.OnClick with
      | Some onClick ->
        yield
          Props.OnClick
            (fun e ->
              let action = onClick e

              dispatch action
            )
          :> Props.IHTMLProp
      | None -> ()

      match connections.OnChange with
      | Some onChange ->
        yield
          Props.OnChange
            (fun e ->
              let action = onChange e

              dispatch action
            )
          :> Props.IHTMLProp
      | None -> ()
    }
    |> Seq.toList

  let reactElementChildren =
    children
    |> List.map (renderConcurElement dispatch)

  reactElementFn reactProps reactElementChildren

type private Props<'tstate> =
  {
    App : ConcurApp<'tstate, Unit>
    InitialState : 'tstate
  }

type private State<'tstate> =
  {
    State : 'tstate
  }

type private ConcurDriver<'tstate> (initProps : Props<'tstate>) =
  inherit Component<Props<'tstate>, State<'tstate>> (initProps) with
    do
      base.setInitState
        {
          State = initProps.InitialState
        }

    override this.componentDidMount () =
      ()

    override this.componentDidUpdate (prevProps, prevState) =
      ()

    override this.componentWillUnmount () =
      ()

    override this.render () =
      let state = this.state.State

      let concurElement = this.props.App state

      let rec dispatch action =
        match action with
        | SetState map ->
          this.setState (fun s _ -> { State = map s.State })
        | ConsoleLog x ->
          printfn "%s" x
        | Output _ -> ()
        | Sequence xs ->
          for x in xs do
            dispatch x

      let renderActions =
        concurElement
        |> collectRenderActions state

      for renderAction in renderActions do
        dispatch renderAction

      let reactElement = renderConcurElement dispatch concurElement

      reactElement

let inline private concurDriver' (app : ConcurApp<'tstate, Unit>) (initialState : 'tstate) =
  ofType<ConcurDriver<_>, Props<_>, State<_>> { App = app; InitialState = initialState } []

let concurDriver (app : ConcurApp<'tstate, Unit>) (initialState : 'tstate) =
  concurDriver' app initialState
