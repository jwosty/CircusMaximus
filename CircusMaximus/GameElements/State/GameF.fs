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
      let (gameOption: option<GameState * GameSounds>) =
        match game.gameState with
        | GameMainMenu mainMenu ->
          match MainMenu.next mainMenu (lastMouse, mouse) (lastKeyboard, keyboard) (lastGamepads, gamepads) with
          | Some(gameState) -> Some(gameState, game.gameSounds)
          | None -> None
        
        | GameTutorial tutorial ->
          let tutorial, gameSounds = Tutorial.next (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.settings game.gameSounds tutorial
          Some(GameTutorial(tutorial), gameSounds)
        
        | GameHorseScreen(playerHorses, buttonGroup) ->
          let buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads buttonGroup
          let gameState =
            match buttonGroup.buttons.[0].buttonState with
            | Releasing -> GamePreRace(playerHorses)
            | _ -> GameHorseScreen(playerHorses, buttonGroup)
          Some(gameState, game.gameSounds)
        
        | GameRace oldRace ->
          let race, gameSounds = Race.next oldRace mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) game.rand game.gameSounds game.settings
          Some(race, gameSounds)
          
        | GameAwardScreen awardScreen ->
          let gameState = AwardScreen.next awardScreen (lastKeyboard, keyboard) mouse gamepads
          Some(gameState, game.gameSounds)
      
      match gameOption with
      | Some(gameState, gameSounds) ->
        match gameState with
        | GamePreMainMenu -> Some({ game with gameState = GameMainMenu(MainMenu.init game.settings); gameSounds = gameSounds })
        | GamePreHorseScreen -> Some(Game.switchToHorseScreen game gameSounds)
        | GamePreRace(playerHorses) -> Some({ game with gameState = GameRace(Race.init playerHorses game.settings); gameSounds = gameSounds })
        | GamePreAwardScreen(race) -> Some(Game.switchToAwardsScreen PlayerData.playerWinnings PlayerData.findByNumber game gameSounds)
        | _ -> Some({ game with gameState = gameState; gameSounds = gameSounds })
      | None -> None