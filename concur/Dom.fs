module Concur.Dom

open Fable.Core
open Fable.React
open Browser.Types
open Concur.Driver

let runApp (container : Element) (app : ConcurApp) =
  ReactDom.render (concurDriver app, container)

