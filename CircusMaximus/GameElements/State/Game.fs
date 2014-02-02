namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type GameState =
  | MainMenu of MainMenu
  | HorseScreen of Horses list * Button
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
  /// Initializes a game state with the given random number generator and window dimensions
  let init rand startingWindowDimensions =
    let settings =
      { windowDimensions = startingWindowDimensions
        debugDrawBounds = true }
    { gameState = MainMenu(MainMenu.init settings)
      settings = settings
      rand = rand
      playerData = List.init Player.numPlayers (fun i -> PlayerData.initEmpty (i + 1))
      gameSounds = GameSounds.allStopped Player.numPlayers }
  
  /// Initializes the game state to a HorseScreen
  let switchToHorseScreen game gameSounds =
    let horses = List.init Player.numPlayers (fun i ->
      let values =
        let ump = Player.unbalanceMidPoint * 100.0 |> int
        repeat (unbalanceRandom 0 (Player.maxStatUnbalance * 100. |> int) game.rand) [ump; ump; ump] Player.unbalanceTimes
        |> List.map (fun n -> float n / 100.0)
      let h =
        { acceleration = Player.baseAcceleration * values.[0]
          topSpeed = Player.baseTopSpeed * values.[1]
          turn = Player.baseTurn * values.[2]}
      printfn "Player %i\n\tacceleration = %f\n\ttop speed = %f\n\tturn = %f" (i + 1) h.acceleration h.topSpeed h.turn
      h)
    { game with
        gameState = HorseScreen(horses, Button.initCenter (game.settings.windowDimensions / (2 @@ 8)) Button.defaultButtonSize "Contine")
        gameSounds = gameSounds }
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      let gameState, gameSounds =
        match game.gameState with
        | MainMenu mainMenu ->
          let gameState =
            (MainMenu.next mainMenu (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepads))
            |> ScreenStatus.map MainMenu
          gameState, game.gameSounds
        
        | HorseScreen(playerHorses, continueButton) ->
          let continueButton = Button.next continueButton mouse
          let gameState =
            match continueButton.buttonState with
            | Releasing -> SwitchToRaces(playerHorses)
            | _ -> NoSwitch(HorseScreen(playerHorses, continueButton))
          gameState, game.gameSounds
        
        | Race oldRace ->
          let raceScreenStatus, gameSounds = Race.next oldRace mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.gameSounds game.settings
          let gameState =
            raceScreenStatus |> ScreenStatus.map
              (fun race ->
                match race.raceState, race.timer with
                | PostRace _, 0 -> ()
                | _ -> ()
                Race(race))
          gameState, gameSounds
          
        | AwardScreen awardScreen ->
          ScreenStatus.map AwardScreen (AwardScreen.next awardScreen mouse),
          game.gameSounds
      
      match gameState with
      | NoSwitch gameState -> Some({ game with gameState = gameState; gameSounds = gameSounds })
      | SwitchToHorseScreen -> Some(switchToHorseScreen game gameSounds)
      | SwitchToMainMenu -> Some({ game with gameState = MainMenu(MainMenu.init game.settings); gameSounds = gameSounds })
      | SwitchToRaces(playerHorses) -> Some({ game with gameState = Race(Race.init playerHorses game.settings); gameSounds = gameSounds })
      | SwitchToAwards ->
        // Update player data
        let players =
          match game.gameState with
          | Race race -> race.players
          | _ -> failwith "The awards screen can only be accessed after a race ends"
        let playerDataAndWinnings =
          players |> List.map
            (fun player ->
              let winnings =
                match player.finishState with
                | Finished placing -> PlayerData.playerWinnings placing
                // Something strange is happening if there's an unfinished player after the race has ended
                | Racing -> 0
              PlayerData.findByNumber player.number game.playerData, winnings)
        let playerHorses = players |> List.map (fun player -> player.horses)
        let awardScreen, playerData = AwardScreen.init game.settings playerDataAndWinnings playerHorses
        Some({ game with gameState = AwardScreen(awardScreen); gameSounds = gameSounds; playerData = playerData })
      | NativeExit -> None