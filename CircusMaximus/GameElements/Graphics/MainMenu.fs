module CircusMaximus.Graphics.MainMenuGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types

/// Draws a main menu
let draw assets (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (mainMenu: MainMenu) =
  fontBatch.DoWithPointClamp (fun fb -> generalBatch.DoBasic (fun gb ->
    List.iter
      (fun button -> ButtonGraphics.draw fb gb button assets)
      mainMenu.buttonGroup.buttons))