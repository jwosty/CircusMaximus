namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type MainMenu =
  { buttonGroup: ButtonGroup }
  
  /// Initializes a new main menu using the given game settings
  static member init (settings: GameSettings) : MainMenu =
    let inline initb y label =
      Button.initCenter
        (settings.windowDimensions * (0.5 @@ y))
        Button.defaultButtonSize label
    { buttonGroup =
        ButtonGroup.init [ initb 0.25 "Disce"; initb 0.5 "Incipe"; initb 0.75 "Exi" ] }