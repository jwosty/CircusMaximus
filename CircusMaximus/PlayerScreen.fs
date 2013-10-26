module CircusMaximus.PlayerScreen
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type PlayerScreen = SpriteBatch * Rectangle

let rasterizerState = new RasterizerState(ScissorTestEnable = true)

// Creates one of 5 possible screens
let createScreen graphics playerNumber =
  let spriteBatch = new SpriteBatch(graphics)
  let w, h = graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight
  let rx, ry, rw, rh =
    match playerNumber with
      // Top screens
      | 0 -> 0, 0,      w / 2, h / 2
      | 1 -> w / 2, 0,  w / 2, h / 2
      // Bottom screens
      | 2 -> 0, h / 2,          w / 3, h / 2
      | 3 -> w / 3, h / 2,      w / 3, h / 2
      | 4 -> w * 2 / 3, h / 2,  w / 3, h / 2
      | _ -> raise (new ArgumentException("player number"))
  spriteBatch, new Rectangle(rx, ry, rw, rh)

// Returns a list of player screens
let createScreens graphics = List.init 5 (createScreen graphics)

let screenDraw drawPredicate (player: Player.Player) (screen: PlayerScreen) =
  let sb, rect = screen
  let oldRect = sb.GraphicsDevice.ScissorRectangle
  sb.Begin(
    SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState, null,
    Matrix.CreateTranslation(
      float32 rect.X + (float32 rect.Width / 2.0f) - player.position.X,
      float32 rect.Y + (float32 rect.Height / 2.0f) - player.position.Y, 0.0f))
  sb.GraphicsDevice.ScissorRectangle <- rect
  drawPredicate screen
  sb.End()