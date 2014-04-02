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
      | SwitchToHorseScreen -> Some(Game.switchToHorseScreen game gameSounds)
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