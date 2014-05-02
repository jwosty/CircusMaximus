namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AwardScreen =
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) (lastKeyboard, keyboard) mouse gamepads =
    let inline buttonState label = ButtonGroup.buttonState awardScreen.buttonGroup label
    match buttonState "Contine", buttonState "Exi cursus" with
    | Releasing, _ -> GamePreRace(awardScreen.playerHorses)
    | _, Releasing -> GamePreMainMenu
    | _ ->
      { awardScreen with
          timer = awardScreen.timer + 1
          buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads awardScreen.buttonGroup }
      |> GameAwardScreen