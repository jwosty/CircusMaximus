namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

type GameState = | MainMenu | Race of Race

/// Holds the state of the entire game
type Game = { rand: Random; gameState: GameState }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Game =
  let init rand = { rand = rand; gameState = MainMenu }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) assets =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      match game.gameState with
      | MainMenu -> Some(game)  // Main menu not implemented yet
      | Race race -> Some({game with gameState = Race(Race.next race (lastKeyboard, keyboard) (lastGamepads, gamepad) game.rand assets)})