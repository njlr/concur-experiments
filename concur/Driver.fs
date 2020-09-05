module Concur.Driver

open System.Threading
open FSharp.Control
open Fable.Core
open Fable.React
open Concur

type private Props =
  {
    App : ConcurApp
  }

type private State =
  {
    View : ReactElement
  }

type private ConcurDriver (initProps : Props) =
  inherit Component<Props, State> (initProps) with
    let mutable cts = new CancellationTokenSource ()

    do
      base.setInitState
        { View = fragment [] [] }

    member private this.Start () =
      Async.StartAsPromise (
        async {
          for view in this.props.App do
            this.setState (fun s _ -> { s with View = view })
        },
        cts.Token
      )
      |> ignore

    override this.componentDidMount () =
      this.Start ()

    override this.componentDidUpdate (prevProps, _) =
      if prevProps.App <> this.props.App
      then
        cts.Cancel ()
        cts <- new CancellationTokenSource ()
        this.Start ()

    override this.componentWillUnmount () =
      cts.Cancel ()
      cts.Dispose ()

    override this.render () =
      this.state.View

let inline private concurDriver' (app : ConcurApp) =
  ofType<ConcurDriver, Props, State> { App = app } []

let concurDriver (app : ConcurApp) = concurDriver' app
