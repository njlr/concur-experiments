module Concur.Dom

open Fable.Core
open Fable.React
open Browser.Types
open Concur.Driver

let runApp (container : Element) (app : ConcurApp<_>) initialState =
  ReactDom.render (concurDriver app initialState, container)
