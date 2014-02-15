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
let drawGame assets generalBatch fontBatch windowCenter windowRect playerScreens (game: Game) =
  match game.gameState with
  | MainMenu mainMenu ->                    MainMenuGraphics.draw assets fontBatch generalBatch mainMenu
  | Tutorial(Tutorial.Tutorial(players)) -> WorldGraphics.drawScreens assets fontBatch playerScreens game.settings players
  | HorseScreen(horses, buttonGroup) ->     HorseScreenGraphics.draw fontBatch generalBatch game buttonGroup assets horses
  | Race race ->                            RaceGraphics.drawRace assets generalBatch fontBatch windowCenter windowRect playerScreens game.settings race
  | AwardScreen awardScreen ->              AwardScreenGraphics.draw assets fontBatch generalBatch game.settings awardScreen