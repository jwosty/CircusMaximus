namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MainMenu =
  /// Updates the main menu
  let next (mainMenu: MainMenu) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    let inline buttonState label = ButtonGroup.buttonState mainMenu.buttonGroup label
    match buttonState "Exi" with
    | Releasing -> NativeExit
    | _ ->
      match buttonState "Disce" with
      | Releasing -> SwitchToTutorial
      | _ ->
        match buttonState "Incipe" with
        | Releasing -> SwitchToHorseScreen
        | _ ->
          NoSwitch(
            { mainMenu with
                buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads mainMenu.buttonGroup })