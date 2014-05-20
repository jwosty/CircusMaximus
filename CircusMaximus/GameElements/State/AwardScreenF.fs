﻿namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input
open CircusMaximus.Types

module AwardScreen =
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) fields ((lastMouse, mouse), (lastKeyboard, keyboard), (lastGamepads, gamepads)) =
    let inline buttonState label = ButtonGroup.buttonState awardScreen.buttonGroup label
    match buttonState "Contine", buttonState "Exi cursus" with
    | Releasing, _ -> Some((Race.init awardScreen.playerHorses fields.settings) :> IGameScreen, fields)
    | _, Releasing -> Some(upcast (MainMenu.init fields.settings), fields)
    | _ ->
      Some(
        upcast new AwardScreen(
          awardScreen.timer + 1, awardScreen.playerDataAndWinnings, awardScreen.playerHorses,
          ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads awardScreen.buttonGroup),
        fields)