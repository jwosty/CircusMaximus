module CircusMaximus.PlayerScreen
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type PlayerScreen = SpriteBatch * Rectangle

let rasterizerState = new RasterizerState(ScissorTestEnable = true)

// Creates one of the 5 screens
let createScreen graphics playerNumber =
  // Each screen doesn't really need it's own sprite batch...
  let spriteBatch = new SpriteBatch(graphics)
  // The game window's width and height
  let w, h = graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight
  let halfW, thirdW, halfH = w / 2, w / 3, h / 2
  // Calculate the screen's position and dimensions
  let rx, ry, rw, rh =
    match playerNumber with
      // Top screens
      | 0 -> 0, 0,      halfW, halfH
      | 1 -> halfW, 0,  halfW, halfH
      // Bottom screens
      | 2 -> 0, halfH,          thirdW, halfH
      | 3 -> thirdW, halfH,     thirdW, halfH
      | 4 -> thirdW * 2, halfH, thirdW, halfH
      | _ -> raise (new ArgumentException("player number"))
  spriteBatch, new Rectangle(rx, ry, rw, rh)

// Returns a list of player screens
let createScreens graphics = List.init 5 (createScreen graphics)

// Draw a single player's screen
let drawSingle drawPredicate (player: Player.Player) (screen: PlayerScreen) =
  let sb, rect = screen
  sb.Begin(
    SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState, null,
    // Use a simple translation matrix based on the player's position to produce scrolling
    Matrix.CreateTranslation(
      float32 rect.X + (float32 rect.Width / 2.0f) - player.position.X,
      float32 rect.Y + (float32 rect.Height / 2.0f) - player.position.Y, 0.0f))
  // Cuts off anything outside the screen's bounds, thus stopping screens from drawing on top of each other
  sb.GraphicsDevice.ScissorRectangle <- rect
  // Call the custom drawing code
  drawPredicate screen
  sb.End()