/// Functions involved in drawing buttons
module CircusMaximus.Graphics.ButtonGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.State

/// Draws a button state, assuming SpriteBatch.Begin has been called
let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (button: Button) (assets: GameContent) =
  let color = if button.isSelected then Color.LawnGreen else Color.White
  generalBatch.Draw(assets.Button, button.position, Color.White)
  FlatSpriteFont.drawString
    assets.Font fontBatch button.label
    (button.position.X + (float32 button.width / 2.0f) @@ button.position.Y + (float32 button.height / 2.0f))
    4.0f color (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center)