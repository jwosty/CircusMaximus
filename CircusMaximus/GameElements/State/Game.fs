namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type GameState =
  | Screen of Screen
  | Race of Race
  | AwardScreen of AwardScreen

/// Holds the state of the entire game
type Game =
    { gameState: GameState
      settings: GameSettings
      rand: Random
      playerData: PlayerData list
      gameSounds: GameSounds }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Game =
  let init rand startingWindowDimensions =
    let settings = { windowDimensions = startingWindowDimensions }
    { gameState = Screen(Screen.initMainMenu settings)
      settings = settings
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1))
      gameSounds = GameSounds.allStopped Player.numPlayers }
  
  let switchToMainMenu game gameSounds = { game with gameState = Screen(Screen.initMainMenu game.settings); gameSounds = gameSounds }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      let gameState, gameSounds, playerData =
        match game.gameState with
        | Screen screen ->
          let gameState =
            Screen.next screen (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepads)
            |> ScreenStatus.map Screen
          gameState, game.gameSounds, game.playerData
        
        | Race oldRace ->
          let raceScreenStatus, gameSounds = Race.next oldRace mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.gameSounds game.settings
          let playerDataRef = ref game.playerData
          let gameState =
            raceScreenStatus |> ScreenStatus.map
              (fun race ->
                let playerData =
                  // Add winnings if the race just ended (last state was MidRace, but current is now PostRace)
                  match oldRace.raceState, race.raceState with
                  | MidRace _, PostRace _ ->
                    // Transform all players' data to reward them if they did well
                    game.playerData |>
                      List.map (fun playerData ->
                        // Find the race player attatched to the data
                        let player = Race.findPlayerByNumber playerData.number oldRace
                        match player.finishState with
                        | Finished placing -> { playerData with coinBalance = PlayerData.playerWinnings placing }
                        // Something strange is happening if there's an unfinished player in a post-race state
                        | _ -> playerData)
                  | _ -> game.playerData
                playerDataRef := playerData
                
                match race.raceState, race.timer with
                | PostRace _, 0 -> ()
                | _ -> ()
                Race(race))
          gameState, gameSounds, !playerDataRef
          
        | AwardScreen awardScreen ->
          ScreenStatus.map AwardScreen (AwardScreen.next awardScreen),
          game.gameSounds, game.playerData
      
      match gameState with
      | NoSwitch gameState -> Some({ game with gameState = gameState; gameSounds = gameSounds; playerData = playerData })
      | SwitchToMainMenu -> Some({ game with gameState = Screen(Screen.initMainMenu game.settings); gameSounds = gameSounds; playerData = playerData })
      | SwitchToRaces -> Some({ game with gameState = Race(Race.init game.settings); gameSounds = gameSounds; playerData = playerData })
      | SwitchToAwards -> Some({ game with gameState = AwardScreen(AwardScreen.initted); gameSounds = gameSounds; playerData = playerData })
      | NativeExit -> None