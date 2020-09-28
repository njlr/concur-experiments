module Concur.Driver

open System.Threading
open FSharp.Control
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Fable.React
open Concur

let rec private renderConcurElement (handleAction) (concurElement : ConcurElement<_>) =
  match concurElement with
  | Simple reactElement -> reactElement
  | Connected (connections, reactElementFn, children) ->
    let reactProps =
      seq {
        match connections.OnClick with
        | Some onClick ->
          yield
            Props.OnClick
              (fun e ->
                let actions = onClick e

                for action in actions do
                  handleAction action
              )
            :> Props.IHTMLProp
        | None -> ()
      }
      |> Seq.toList

    let reactElementChildren =
      children
      |> List.map (renderConcurElement handleAction)

    reactElementFn reactProps reactElementChildren

type private Props<'tstate> =
  {
    App : ConcurApp<'tstate>
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
      let concurElement, actions = this.props.App this.state.State

      let handleAction action =
        match action with
        | SetState nextState ->
          this.setState (fun _ _ -> { State = nextState })
        | ConsoleLog x ->
          printfn "%s" x

      for action in actions do
        handleAction action

      let reactElement = renderConcurElement handleAction concurElement

      reactElement

let inline private concurDriver' (app : ConcurApp<'tstate>) (initialState : 'tstate) =
  ofType<ConcurDriver<_>, Props<_>, State<_>> { App = app; InitialState = initialState } []

let concurDriver (app : ConcurApp<'tstate>) (initialState : 'tstate) =
  concurDriver' app initialState
