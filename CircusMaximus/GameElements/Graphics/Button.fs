/// Functions involved in drawing buttons
module CircusMaximus.Graphics.ButtonGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

/// Draws a button state, assuming SpriteBatch.Begin has been called
let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (button: Button) (assets: GameContent) =
  generalBatch.Draw(assets.Button, xnaVec2 button.position, Color.White)
  FlatSpriteFont.drawString
    assets.Font fontBatch button.label
    (button.position + (button.dimensions / 2))
    4.0 Color.White (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center)
  if button.isSelected then
    let center = button.position + (button.dimensions / 2)
    generalBatch.DrawCentered(assets.ButtonSelector, xnaVec2 center, Color.White)