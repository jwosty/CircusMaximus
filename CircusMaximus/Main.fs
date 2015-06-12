module CircusMaximus.Main

[<EntryPoint>]
let main args =
  use gameWindow = new GameWindow()
  gameWindow.Run ()
  0