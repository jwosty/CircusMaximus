module CircusMaximus.Player
open System

// =====================
// == XNA INDEPENDENT ==
// =====================

type Player =
  struct
    // This is from XNA, but it's a common mathematical structure so it's OK
    val public position: Microsoft.Xna.Framework.Vector2
    // Radians
    val public direction: float
    val public velocity: float
    val public lap: int
    new(p, d, v, l) = { position = p; direction = d; velocity = v; lap = l }
  end

#nowarn "49"
// Returns a new player updated with the given parameters
let update (Δdirection, velocity) (player: Player) =
  let x, y = cos player.position.X, sin player.position.Y
  new Player(
    new Microsoft.Xna.Framework.Vector2(
      // Go forward
      player.position.X + float32 (cos player.direction * player.velocity),
      player.position.Y + float32 (sin player.direction * player.velocity)),
    // Turn and de/accellerate
    player.direction + Δdirection, velocity, player.lap)


// ===================
// == XNA DEPENDENT ==
// ===================

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Input

let degreesToRadians d = 2.0 * Math.PI / 360.0 * d

let private maxTurn, maxSpeed = 2.0, 4.0

// Returns change in direction and power (in that order) based on the given game pad state
let getPowerTurnFromGamepad(gamepad: GamePadState) =
  (float gamepad.ThumbSticks.Left.X * maxTurn |> degreesToRadians, float gamepad.Triggers.Right * maxSpeed)

// Returns change in direction and power (in that order) based on the given keyboard state
let getPowerTurnFromKeyboard(keyboard: KeyboardState) =
  ( (if keyboard.IsKeyDown(Keys.A) then -maxTurn else 0.0) + (if keyboard.IsKeyDown(Keys.D) then maxTurn else 0.0)
      |> degreesToRadians,
    (if keyboard.IsKeyDown(Keys.W) then maxSpeed else 0.0))


let loadContent (content: ContentManager) =
  content.Load<Texture2D>("chariot")

// Renders a player, assuming spriteBatch.Begin has already been called
let draw (player: Player) (spriteBatch: SpriteBatch) (texture: Texture2D) =
  spriteBatch.Draw(
    texture, player.position, new Nullable<_>(), Color.White, single player.direction,
    new Vector2(float32 texture.Width / 1.75f, float32 texture.Height / 1.75f), // texture center
    0.25f, // scale
    SpriteEffects.None, single 0)