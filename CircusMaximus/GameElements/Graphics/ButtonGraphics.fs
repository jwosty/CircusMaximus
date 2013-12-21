/// Functions involved in drawing buttons
module CircusMaximus.Graphics.ButtonGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.State

/// Draws a button state
let draw (sb: SpriteBatch) (button: Button) (assets: GameContent) = sb.Draw(assets.Button, button.position, Color.White)