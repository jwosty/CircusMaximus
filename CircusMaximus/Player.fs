module CircusMaximus.Player
open System
// ===========
// == Logic ==
// ===========
open Microsoft.Xna.Framework

type Player =
  struct
    val public position: Vector2
    // Radians
    val public direction: float
    val public velocity: float
    new(p, d, v) = { position = p; direction = d; velocity = v }
  end
  

// Returns a new player updated appropriately
let update (player: Player, turn, acceleration) =
  let x, y = cos player.position.X, sin player.position.Y
  new Player(
    new Vector2(
      // Go forward
      player.position.X + (cos player.position.X * float32 player.velocity),
      player.position.Y + (sin player.position.Y * float32 player.velocity)),
    // Turn and de/accellerate
    player.direction + turn, (player.velocity + acceleration) / 2.0)

// ==============
// == GRAPHICS ==
// ==============
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content

let loadContent (content: ContentManager) =
  content.Load<Texture2D>("chariot")

// Renders a player, assuming spriteBatch.Begin has already been called
let draw (player: Player) (spriteBatch: SpriteBatch) (texture: Texture2D) =
  spriteBatch.Draw(texture, player.position, new Nullable<_>(), Color.White, single 0, Vector2.Zero, single 1, SpriteEffects.None, single 0)