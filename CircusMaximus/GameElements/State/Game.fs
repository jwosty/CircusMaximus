namespace CircusMaximus.Types
open System
open System.Collections.Generic
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// Holds the state of the entire game
type Game =
  { gameScreen: IGameScreen
    fields: GameFields }
  
  /// Initializes a game state with the given random number generator and window dimensions
  static member init rand startingWindowDimensions =
    let settings =
      { windowDimensions = startingWindowDimensions
        debugDrawBounds = false
        debugLapIncrement = false }
    { gameScreen = MainMenu.init settings
      fields =
        { settings = settings
          rand = rand
          sounds = GameSounds.allStopped Player.numPlayers
          playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1)) } }