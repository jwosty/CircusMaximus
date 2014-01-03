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
  | AwardScreen of int

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
    { gameState = AwardScreen(0)//Screen(Screen.initMainMenu settings)
      settings = settings
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1))
      gameSounds = GameSounds.allStopped Player.numPlayers }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      match game.gameState with
      | Screen screen ->
        // Update the screen and proceed to stay on this screen or switch to something else
        match Screen.next screen (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepads) with
        | NoSwitch screen -> Some({ game with gameState = Screen(screen) })   // Continue updating the screen
        | SwitchToRaces -> Some({ game with gameState = Race(Race.init game.settings) })  // Initialize a new race
        | NativeExit -> None  // Indicate that we want to exit
      
      | Race oldRace ->
        match Race.next oldRace mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.gameSounds game.settings with
        | Some race, gameSounds ->
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
                  | Finished placing -> PlayerData.awardWinnings playerData placing
                  // Something strange is happening if there's an unfinished player in a post-race state
                  | _ -> playerData)
            | _ -> game.playerData
          // Tie all this data together into an updated game state
          let game =
            { game with
                gameState = Race(race)
                playerData = playerData
                gameSounds = gameSounds }
          
          match race.raceState, race.timer with
          | PostRace _, 0 -> ()
          | _ -> ()
          Some(game)
        | None, gameSounds ->
          Some(
            { game with
                gameState = Screen(Screen.initMainMenu game.settings)   // The races have been exited and we need to return to the main menu
                gameSounds = gameSounds })
      
      | AwardScreen timer -> Some({ game with gameState = AwardScreen(timer + 1) })