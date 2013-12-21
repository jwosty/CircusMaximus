namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type GameState =
  | MainMenu of Button  // For now, the main menu only has a single button
  | Race of Race

/// Holds the state of the entire game
type Game = { rand: Random; gameState: GameState }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Game =
  let init rand = { rand = rand; gameState = MainMenu(Button.init (0 @@ 0)) }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) assets =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      match game.gameState with
      | MainMenu button ->
        Some({game with gameState = MainMenu(Button.next button lastKeyboard keyboard)})
      | Race race -> Some({game with gameState = Race(Race.next race (lastKeyboard, keyboard) (lastGamepads, gamepad) game.rand assets)})