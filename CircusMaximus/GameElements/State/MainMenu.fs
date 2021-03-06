namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type MainMenu(buttonGroup: ButtonGroup) =
  member this.buttonGroup = buttonGroup

  interface IGameScreen with
    member this.Next deltaTime rand input = MainMenu.next this deltaTime rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  /// Initializes a new main menu using the given game settings
  static member init (settings: GameSettings) =
    let initb y label =
      Button.initCenter
        (settings.windowDimensions * (0.5 @@ y))
        Button.defaultButtonDimensions label
    new MainMenu(ButtonGroup.init [ initb 0.33 "Incipe"; initb 0.66 "Exi" ])