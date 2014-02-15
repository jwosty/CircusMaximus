/// A module to draw game states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.GameGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

/// Draw a game state
let draw assets generalBatch fontBatch windowCenter windowRect playerScreens (game: Game) =
  match game.gameState with
  | MainMenu mainMenu ->                MainMenuGraphics.draw     assets fontBatch generalBatch mainMenu
  | Tutorial tutorial ->                TutorialGraphics.draw     assets fontBatch playerScreens game.settings tutorial
  | HorseScreen(horses, buttonGroup) -> HorseScreenGraphics.draw  assets fontBatch generalBatch horses buttonGroup game
  | Race race ->                        RaceGraphics.draw         assets generalBatch fontBatch windowCenter windowRect playerScreens game.settings race
  | AwardScreen awardScreen ->          AwardScreenGraphics.draw  assets generalBatch fontBatch game.settings awardScreen