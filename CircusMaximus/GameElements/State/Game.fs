namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type GameState =
  | GameMainMenu of MainMenu
  | GameTutorial of Tutorial
  | GameHorseScreen of Horses list * ButtonGroup
  | GameRace of Race
  | GameAwardScreen of AwardScreen

/// Holds the state of the entire game
type Game =
  { gameState: GameState
    settings: GameSettings
    rand: Random
    playerData: PlayerData list
    gameSounds: GameSounds }
  
  /// Initializes a game state with the given random number generator and window dimensions
  static member init rand startingWindowDimensions =
    let settings =
      { windowDimensions = startingWindowDimensions
        debugDrawBounds = false
        debugLapIncrement = false }
    { gameState = GameMainMenu(MainMenu.init settings)
      settings = settings
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1))
      gameSounds = GameSounds.allStopped Player.numPlayers }
  
  /// Initializes the HorseScreen game state
  static member switchToHorseScreen game gameSounds =
    let horses = List.init Player.numPlayers (fun i ->
      let values =
        let ump = Player.unbalanceMidPoint * 100.0 |> int
        repeat (unbalanceRandom 0 (Player.maxStatUnbalance * 100. |> int) game.rand) [ump; ump; ump] Player.unbalanceTimes
        |> List.map (fun n -> float n / 100.0)
      { acceleration = Player.baseAcceleration * values.[0]
        topSpeed = Player.baseTopSpeed * values.[1]
        turn = Player.baseTurn * values.[2]})
    { game with
        gameState =
          GameHorseScreen(
            horses,
            ButtonGroup.init(
              [ Button.initCenter (game.settings.windowDimensions / (2 @@ 8)) Button.defaultButtonSize "Contine" ]))
        gameSounds = gameSounds }