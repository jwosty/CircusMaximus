module CircusMaximus.Graphics.MainMenuGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

/// Draws a main menu
let draw (mainMenu: MainMenu) (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) assets =
  fontBatch.DoWithPointClamp
    (fun fb ->
      generalBatch.DoBasic (fun gb ->
        ButtonGraphics.draw fb gb mainMenu.playButton assets
        ButtonGraphics.draw fb gb mainMenu.quitButton assets))