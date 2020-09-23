module Concur.Driver

open System.Threading
open FSharp.Control
open Fable.Core
open Fable.Core.JS
open Fable.React
open Concur
open Browser

type private Props<'state> =
  {
    InitialState : 'state
    App : ConcurApp<'state>
  }

type private State<'state> =
  {
    State : 'state
  }

type private ConcurDriver<'state> (initProps : Props<'state>) =
  inherit Component<Props<'state>, State<'state>> (initProps) with
    let mutable cts = new CancellationTokenSource ()

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
      cts.Cancel ()
      cts.Dispose ()

    override this.render () =
      let next = this.props.App this.state.State
      let reactElement, actions = next

      for action in actions do
        match action with
        | NoOp -> ()
        | ConsoleLog text ->
          printfn "%s" text
        | Sleep (ms, mapState) ->
          setTimeout
            (fun _ -> this.setState (fun s p -> { s with State = mapState s.State }))
            ms
          |> ignore
        | _ ->
          printfn "Action: %A" action

      reactElement (fun nextState -> this.setState (fun s p -> { s with State = nextState }))

let inline private concurDriver' (app : ConcurApp<_>) initialState =
  ofType<ConcurDriver<_>, Props<_>, State<_>> { App = app; InitialState = initialState } []

let concurDriver (app : ConcurApp<_>) initialState = concurDriver' app initialState
