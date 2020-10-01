module Concur.Components

open Fable.React
open Concur

let h1 props children = (ConcurElement.wrapReact h1) props children

let h2 props children = (ConcurElement.wrapReact h2) props children

let p props children = (ConcurElement.wrapReact p) props children

let s props children = (ConcurElement.wrapReact s) props children

let ul props children = (ConcurElement.wrapReact ul) props children

let li props children = (ConcurElement.wrapReact li) props children

let div props children = (ConcurElement.wrapReact div) props children

let form props children = (ConcurElement.wrapReact form) props children

let button props children = (ConcurElement.wrapReact button) props children

let input props = ConcurElement.wrapReact (fun injectedProps _ -> input (injectedProps @ props)) [] []

let textarea props children = (ConcurElement.wrapReact textarea) props children

let str text = ConcurElement.wrapReact (fun _ _ -> str text) [] []
