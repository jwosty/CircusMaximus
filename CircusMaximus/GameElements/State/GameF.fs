﻿namespace CircusMaximus.Functions
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

module Game =
  /// Sets up game screen update functions -- MUST be called before game is started!
  let initFunctions () =
    // TODO: Find a better way to do this... Maybe with some clever reflection and/or Attributes?
    Types.HorseScreen.next <- HorseScreen.next
    Types.Tutorial.next <- Tutorial.next
    Types.Race.next <- Race.next
    Types.MainMenu.next <- MainMenu.next
    Types.AwardScreen.next <- AwardScreen.next
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (deltaTime: float<s>) input =
    if input.keyboard.IsKeyDown Keys.Escape then
      None // Indicate that we want to exit
    else
      game.gameScreen.Next deltaTime game.fields input