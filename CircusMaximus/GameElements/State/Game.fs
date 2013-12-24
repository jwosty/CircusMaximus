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
      rand: Random
      playerData: PlayerData list }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Game =
  let init rand windowDimensions =
    { gameState =
        Screen(
          MainMenu(
            Button.initCenter
              (windowDimensions * (0.5 @@ 0.5))
              (512, 64)
              "Play"))
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1)) }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) assets =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      match game.gameState with
      | Screen oldScreen ->
        let screenOrExit = Screen.next oldScreen (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepad)
        match screenOrExit with
        | Some(screen) -> Some({game with gameState = Screen(screen)})
        | None -> Some({game with gameState = Race(Race.init ())})
      
      | Race oldRace ->
        let race = Race.next oldRace (lastKeyboard, keyboard) (lastGamepads, gamepad) game.rand assets
        let playerData =
          match oldRace.raceState, race.raceState with
          | MidRace _, PostRace ->
            game.playerData |>
              List.map (fun playerData ->
                let player = Race.findPlayerByNumber playerData.number oldRace
                match player.finishState with
                | Finished placing -> PlayerData.awardWinnings playerData placing
                | _ -> playerData)
          | _ -> game.playerData
        
        let game =
          { game with
              gameState = Race(race)
              playerData = playerData }
        
        match race.raceState, race.timer with
        | PostRace, 0 -> ()
        | _ -> ()
        Some(game)