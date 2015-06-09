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
let draw graphics assets generalBatch (fontBatch: SpriteBatch) windowCenter windowRect (game: Game) =
  match game.gameScreen with
  | :? MainMenu as mainMenu ->        MainMenuGraphics.draw     graphics assets generalBatch fontBatch mainMenu
  | :? Tutorial as tutorial ->        TutorialGraphics.draw     graphics assets generalBatch fontBatch game.fields.settings tutorial
  | :? HorseScreen as horseScreen ->  HorseScreenGraphics.draw  graphics assets generalBatch fontBatch horseScreen game
  | :? Race as race ->                RaceGraphics.draw         graphics assets generalBatch fontBatch windowRect game.fields.settings race
  | :? AwardScreen as awardScreen ->  AwardScreenGraphics.draw  graphics assets generalBatch fontBatch game.fields.settings awardScreen
  | _ -> ()