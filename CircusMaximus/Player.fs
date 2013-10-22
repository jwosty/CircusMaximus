module CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

type Player =
  struct
    val public position: Vector2
    val public direction: float
    val public velocity: float
    new(p, d, v) = { position = p; direction = d; velocity = v }
  end

let update (player: Player, turn, acceleration) =
  let x, y = cos player.position.X, sin player.position.Y
  new Player(
    new Vector2(
      player.position.X + (cos player.position.X * float32 player.velocity),
      player.position.Y + (sin player.position.Y * float32 player.velocity)),
    player.direction + turn, (player.velocity + acceleration) / 2.0)