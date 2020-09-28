namespace Concur

open Fable
open Fable.React
open Fable.SimpleHttp
open Browser.Types

type ConcurAction<'tstate> =
  | SetState of 'tstate
  | ConsoleLog of string

type Connections<'tstate> =
  {
    OnClick : (MouseEvent -> ConcurAction<'tstate> list) option
  }

type ConcurElement<'tstate> =
  | Connected of Connections<'tstate> * (Props.IHTMLProp list -> ReactElement list -> ReactElement) * ConcurElement<'tstate> list
  | Simple of ReactElement

type ConcurApp<'tstate> = 'tstate -> ConcurElement<'tstate> * (ConcurAction<'tstate> list)

module ConcurElement =

  let simple (reactElement) : ConcurElement<_> =
    ConcurElement<'tstate>.Simple reactElement

  let connected connections reactElementFn children =
    ConcurElement<'tstate>.Connected (connections, reactElementFn, children)

  let stateless reactElementFn props children =
    connected
      {
        OnClick = None
      }
      (fun reactProps reactChildren -> reactElementFn (props @ reactProps) reactChildren)
      children
