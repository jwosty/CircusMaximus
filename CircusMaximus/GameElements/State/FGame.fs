namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Game =
  /// Initializes a game state with the given random number generator and window dimensions
  let init rand startingWindowDimensions =
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
  let switchToHorseScreen game gameSounds =
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
  
  /// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
  let next (game: Game) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepads) =
    if keyboard.IsKeyDown(Keys.Escape) then
      None  // Indicate that we want to exit
    else
      let gameState, gameSounds =
        match game.gameState with
        | GameMainMenu mainMenu ->
          let gameState =
            (MainMenu.next mainMenu (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepads))
            |> ScreenStatus.map GameMainMenu
          gameState, game.gameSounds
        
        | GameTutorial tutorial ->
          let tutorial, gameSounds = Tutorial.next (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.settings game.gameSounds tutorial
          ScreenStatus.map GameTutorial tutorial, gameSounds
        
        | GameHorseScreen(playerHorses, buttonGroup) ->
          let buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads buttonGroup
          let gameState =
            match buttonGroup.buttons.[0].buttonState with
            | Releasing -> SwitchToRaces(playerHorses)
            | _ -> NoSwitch(GameHorseScreen(playerHorses, buttonGroup))
          gameState, game.gameSounds
        
        | GameRace oldRace ->
          let raceScreenStatus, gameSounds = Race.next oldRace mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.gameSounds game.settings
          let gameState =
            raceScreenStatus
              |> ScreenStatus.map GameRace//(fun race ->
                  //match race.raceState, race.timer with
                  //| PostRace _, 0 -> ()
                  //| _ -> ()
                  //GameRace(race))
          gameState, gameSounds
          
        | GameAwardScreen awardScreen ->
          ScreenStatus.map GameAwardScreen (AwardScreen.next awardScreen (lastKeyboard, keyboard) mouse gamepads),
          game.gameSounds
      
      match gameState with
      | NoSwitch gameState -> Some({ game with gameState = gameState; gameSounds = gameSounds })
      | SwitchToMainMenu -> Some({ game with gameState = GameMainMenu(MainMenu.init game.settings); gameSounds = gameSounds })
      | SwitchToTutorial -> Some({ game with gameState = GameTutorial(Tutorial.init ()) })
      | SwitchToHorseScreen -> Some(switchToHorseScreen game gameSounds)
      | SwitchToRaces(playerHorses) -> Some({ game with gameState = GameRace(Race.init playerHorses game.settings); gameSounds = gameSounds })
      | SwitchToAwards ->
        // Update player data
        let players =
          match game.gameState with
          | GameRace race -> race.players
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
        Some({ game with gameState = GameAwardScreen(awardScreen); gameSounds = gameSounds; playerData = playerData })
      | NativeExit -> None