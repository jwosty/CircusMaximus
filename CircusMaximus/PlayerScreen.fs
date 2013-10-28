module CircusMaximus.PlayerScreen
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type PlayerScreen = SpriteBatch * Rectangle

let rasterizerState = new RasterizerState(ScissorTestEnable = true)

// Compute a rectangle for a single screen
let screenBounds (screenWidth, screenHeight) border (row, column) =
  // Determine how many screens will be in this row
  // If you think about it, the screen arrangement is actually a pyramid (granted, only the top two rows)
  let screensInRow = column + 2.0f
  let screensInColumn = 2.0f
  let halfB = border * 0.5f
  // Do maths ignoring borders
  let bx, by, bw, bh =
    (screenWidth * (row / screensInRow)),
    (screenHeight * (column / screensInColumn)),
    (screenWidth / screensInRow),
    (screenHeight / screensInColumn)
  
  // Subtract borders
  let x, w =
    if row = 0.0f then
      bx, bw - halfB
    elif row = (screensInRow - 1.0f) then
      bx + halfB, bw - halfB
    else
      bx + halfB, bw - border
  let y, h =
    if column = 0.0f then
      by, bh - halfB
    elif column = (screensInColumn - 1.0f) then
      by + halfB, bh - halfB
    else
      by + halfB, bh - border
  
  new Rectangle(int <| round x, int <| round y, int <| round w, int <| round h)

// Creates one of the 5 screens
let createScreen graphics playerNumber =
  // Each screen doesn't really need it's own sprite batch...
  let spriteBatch = new SpriteBatch(graphics)
  let windowWidth, windowHeight = graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight
  // Calculate what the screen's bounds should be
  let bounds =
    screenBounds (float32 windowWidth, float32 windowHeight) 6.0f <|
      match playerNumber with
        // Top screens
        | 0 -> 0.0f, 0.0f
        | 1 -> 1.0f, 0.0f
        // Bottom screens
        | 2 -> 0.0f, 1.0f
        | 3 -> 1.0f, 1.0f
        | 4 -> 2.0f, 1.0f
        | _ -> raise (new ArgumentException("player number"))
  spriteBatch, bounds

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