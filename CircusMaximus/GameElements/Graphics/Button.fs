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
  generalBatch.Draw(assets.Button, button.position, Color.White)
  FlatSpriteFont.drawString
    assets.Font fontBatch button.label
    (button.position.X + (float32 button.width / 2.0f) @@ button.position.Y + (float32 button.height / 2.0f))
    4.0f Color.White (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center)
  if button.isSelected then
    let center = button.position + (float button.width / 2.0 @@ float button.height / 2.0)
    generalBatch.DrawCentered(assets.ButtonSelector, center, Color.White)