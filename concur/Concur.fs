namespace Concur

open Fable.SimpleHttp
open Fable.React

type BrowserAction<'state> =
  | NoOp
  | ConsoleLog of string
  | Fetch of HttpRequest * (HttpResponse -> 'state -> 'state)
  | Sleep of int * ('state -> 'state)
  | SetState of ('state -> 'state)

type Dispatch<'state> = 'state -> Unit

type ConcurApp<'state> = 'state -> (Dispatch<'state> -> ReactElement) * (BrowserAction<'state> list)
