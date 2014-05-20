﻿namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

module MainMenu =
  /// Updates the main menu
  let next (mainMenu: MainMenu) fields input =
    let inline buttonState label = ButtonGroup.buttonState mainMenu.buttonGroup label
    match buttonState "Exi" with
    | Releasing -> None
    | _ ->
      match buttonState "Disce" with
      | Releasing -> Some(Tutorial.init () :> IGameScreen, fields)
      | _ ->
        match buttonState "Incipe" with
        | Releasing ->
          let horseScreen, fields = HorseScreen.init fields
          Some(upcast horseScreen, fields)
        | _ ->
          let buttonGroup = ButtonGroup.next mainMenu.buttonGroup input
          Some(upcast new MainMenu(buttonGroup), fields)