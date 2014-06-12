module CircusMaximus.Main
open CircusMaximus
open CircusMaximus.Types
open System

[<EntryPoint>]
let main argv =
  let game = new GameWindow()
  game.Run ()
  0