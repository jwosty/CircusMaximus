namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input
open CircusMaximus.Types

module HorseScreen =
  let next (horseScreen: HorseScreen) fields input =
    let buttons = ButtonGroup.next horseScreen.buttons input
    let screen =
      match buttons.buttons.[0].buttonState with
      | Releasing -> Race.init horseScreen.horses fields.settings :> IGameScreen
      | _ -> upcast new HorseScreen(horseScreen.horses, buttons)
    Some(screen, fields)