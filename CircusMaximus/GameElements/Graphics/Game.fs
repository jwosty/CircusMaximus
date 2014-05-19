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
  match game.gameScreen with
  | :? MainMenu as mainMenu ->        MainMenuGraphics.draw     assets fontBatch generalBatch mainMenu
  | :? Tutorial as tutorial ->        TutorialGraphics.draw     assets fontBatch playerScreens game.fields.settings tutorial
  | :? HorseScreen as horseScreen ->  HorseScreenGraphics.draw  assets fontBatch generalBatch horseScreen.horses horseScreen.buttons game
  | :? Race as race ->                RaceGraphics.draw         assets generalBatch fontBatch windowCenter windowRect playerScreens game.fields.settings race
  | :? AwardScreen as awardScreen ->  AwardScreenGraphics.draw  assets generalBatch fontBatch game.fields.settings awardScreen
  | _ -> ()