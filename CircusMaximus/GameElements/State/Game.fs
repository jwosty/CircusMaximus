namespace CircusMaximus.Types
open System
open System.Collections.Generic
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

(*
type GameState =
  | GameMainMenu of MainMenu
  | GameTutorial of Tutorial
  | GameHorseScreen of Horses list * ButtonGroup
  | GamePreRace of Horses list
  | GameRace of Race
  | GamePreAwardScreen of Race
  | GameAwardScreen of AwardScreen
*)

/// Holds the state of the entire game
type Game =
  { gameState: IGameScreen
    fields: GameFields }
  
  /// Initializes a game state with the given random number generator and window dimensions
  static member init rand startingWindowDimensions =
    let settings =
      { windowDimensions = startingWindowDimensions
        debugDrawBounds = false
        debugLapIncrement = false }
    { gameState = MainMenu.init settings
      fields =
        { settings = settings
          rand = rand
          sounds = GameSounds.allStopped Player.numPlayers
          playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1)) } }
  
  /// Initializes the HorseScreen game state
  static member switchToHorseScreen horsesNext game gameSounds =
    let horses = List.init Player.numPlayers (fun i ->
      let values =
        let ump = Player.unbalanceMidPoint * 100.0 |> int
        repeat (unbalanceRandom 0 (Player.maxStatUnbalance * 100. |> int) game.fields.rand) [ump; ump; ump] Player.unbalanceTimes
        |> List.map (fun n -> float n / 100.0)
      { acceleration = Player.baseAcceleration * values.[0]
        topSpeed = Player.baseTopSpeed * values.[1]
        turn = Player.baseTurn * values.[2]})
    { gameState =
        HorseScreen.init
          horsesNext horses
          (ButtonGroup.init([ Button.initCenter (game.fields.settings.windowDimensions / (2 @@ 8)) Button.defaultButtonSize "Contine" ]))
      fields = { game.fields with sounds = gameSounds } }
  
  static member switchToAwardsScreen findPlayerWinnings findPlayerDataByNumber game gameSounds =
    // Update player data
    let players =
      match game.gameState with
      :? Race as race -> race.players
      | _ -> failwith "The awards screen can only be accessed after a race ends"
    let playerDataAndWinnings =
      players |> List.map
        (fun player ->
          let winnings =
            match player.finishState with
            | Finished placing -> findPlayerWinnings placing
            // Something strange is happening if there's an unfinished player after the race has ended
            | Racing -> 0
          findPlayerDataByNumber player.number game.fields.playerData, winnings)
    let playerHorses = players |> List.map (fun player -> player.horses)
    let awardScreen, playerData = AwardScreen.init  game.fields playerDataAndWinnings playerHorses
    { gameState = awardScreen
      fields = { game.fields with sounds = gameSounds; playerData = playerData } }