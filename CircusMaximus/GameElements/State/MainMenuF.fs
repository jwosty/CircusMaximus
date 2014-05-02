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
  let next (mainMenu: MainMenu) (lastMouse, mouse) keyboard (lastGamepads, gamepads) =
    let inline buttonState label = ButtonGroup.buttonState mainMenu.buttonGroup label
    match buttonState "Exi" with
    | Releasing -> None
    | _ ->
      match buttonState "Disce" with
      | Releasing -> Some(GameTutorial(Tutorial.init ()))
      | _ ->
        match buttonState "Incipe" with
        | Releasing -> Some(GamePreHorseScreen)
        | _ ->
            let buttonGroup = ButtonGroup.next keyboard mouse gamepads mainMenu.buttonGroup
            Some(GameMainMenu({ mainMenu with buttonGroup = buttonGroup }))