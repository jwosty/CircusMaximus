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
    { gameState =
        Screen(
          MainMenu(
            Button.initCenter
              (settings.windowDimensions * (0.5 @@ 0.5))
              Button.defaultButtonSize "Play"))
      settings = settings
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1))
      gameSounds = GameSounds.allStopped Player.numPlayers }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      let game =  
        match game.gameState with
        | Screen oldScreen ->
          // Some(screen) means that the screen wants to remain and should be updated; None means that the screen is
          // ready to start the game
          let screenOrExit = Screen.next oldScreen (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepad)
          match screenOrExit with
          | Some(screen) -> { game with gameState = Screen(screen) }
          | None -> { game with gameState = Race(Race.init game.settings) }
        
        | Race oldRace ->
          let race, gameSounds = Race.next oldRace (lastKeyboard, keyboard) (lastGamepads, gamepad) game.rand game.gameSounds game.settings
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
          
          let game =
            { game with
                gameState = Race(race)
                playerData = playerData
                gameSounds = gameSounds }
          
          match race.raceState, race.timer with
          | PostRace _, 0 -> ()
          | _ -> ()
          game
      Some(game)