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
  let next (mainMenu: MainMenu) fields (((_, mouse), keyboard, (lastGamepads, gamepads)): GameInput) =
    let inline buttonState label = ButtonGroup.buttonState mainMenu.buttonGroup label
    match buttonState "Exi" with
    | Releasing -> None
    | _ ->
      match buttonState "Disce" with
      | Releasing -> Some(Tutorial.init () :> IGameScreen, fields)
      | _ ->
        match buttonState "Incipe" with
        | Releasing -> Some(upcast MainMenu.init fields.settings, fields)
        | _ ->
            let buttonGroup = ButtonGroup.next keyboard mouse gamepads mainMenu.buttonGroup
            Some(upcast new MainMenu(buttonGroup), fields)
  
  MainMenu.next <- next