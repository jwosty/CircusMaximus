namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type MainMenu = { buttonGroup: ButtonGroup }

/// Contains functions and constants pertaining to main menus
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MainMenu =
  let init (settings: GameSettings) =
    let inline initb y label =
      Button.initCenter
        (settings.windowDimensions * (0.5 @@ y))
        Button.defaultButtonSize label
    { buttonGroup =
        ButtonGroup.init [ initb 0.333 "Incipe"; initb 0.666 "Exi" ] }
  
  /// Updates the main menu
  let next (mainMenu: MainMenu) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    let inline buttonState label = ButtonGroup.buttonState mainMenu.buttonGroup label
    match buttonState "Incipe", buttonState "Exi" with
    | _, Releasing -> NativeExit
    | Releasing, _ -> SwitchToHorseScreen
    | _, _ ->
      NoSwitch(
        { mainMenu with
            buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads mainMenu.buttonGroup })