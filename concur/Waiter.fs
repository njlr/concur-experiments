namespace Concur

open System

type Waiter<'t> () =
  let mutable isDisposed = false
  let mutable result = None
  let mutable callbacks = []

  member this.Resolve (x : 't) =
    result <- Some x

    for (resolve, _) in callbacks do
      resolve x

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
              callbacks <- (resolve, cancel) :: callbacks)

  interface IDisposable with
    override this.Dispose () =
      if not isDisposed
      then
        isDisposed <- true
        result <- None

        for (_, cancel) in callbacks do
          cancel (OperationCanceledException ())

        callbacks <- []
