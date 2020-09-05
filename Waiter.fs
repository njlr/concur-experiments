namespace Concur

open System

type Waiter<'t> () =
  let mutable isDisposed = false
  let mutable result = None
  let mutable callbacks = []
  let mutable cancelCallbacks = []

  member this.Resolve (x : 't) =
    result <- Some x

    for callback in callbacks do
      callback x

    callbacks <- []

  member this.Resolved
    with get () =
      if isDisposed
      then
        async {
          return raise (OperationCanceledException ())
        }
      else
        match result with
        | Some x ->
          async {
            return x
          }
        | None ->
          Async.FromContinuations
            (fun (resolve, reject, cancel) ->
              callbacks <- resolve :: callbacks
              cancelCallbacks <- cancel :: cancelCallbacks)

  interface IDisposable with
    override this.Dispose () =
      if not isDisposed
      then
        isDisposed <- true

        for cancelCallback in cancelCallbacks do
          cancelCallback (OperationCanceledException ())

        callbacks <- []
        cancelCallbacks <- []
