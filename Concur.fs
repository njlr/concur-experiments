namespace Concur

open FSharp.Control
open Fable.React

type ConcurApp = AsyncSeq<ReactElement>

module Dom =

  open Fable.Core
  open Browser.Types

  let runApp (container : Element) (app : ConcurApp) =
    async {
      for view in app do
        ReactDom.render (view, container)
    }
    |> Async.StartAsPromise
    |> ignore

