/// A module to draw game states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.GameGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types

/// Draw a game state
let draw assets generalBatch fontBatch windowCenter windowRect playerScreens (game: Game) =
  match game.gameState with
  | GameMainMenu mainMenu ->                MainMenuGraphics.draw     assets fontBatch generalBatch mainMenu
  | GameTutorial tutorial ->                TutorialGraphics.draw     assets fontBatch playerScreens game.settings tutorial
  | GameHorseScreen(horses, buttonGroup) -> HorseScreenGraphics.draw  assets fontBatch generalBatch horses buttonGroup game
  | GameRace race ->                        RaceGraphics.draw         assets generalBatch fontBatch windowCenter windowRect playerScreens game.settings race
  | GameAwardScreen awardScreen ->          AwardScreenGraphics.draw  assets generalBatch fontBatch game.settings awardScreen