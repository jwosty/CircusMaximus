namespace CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

type CommonPlayerData =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public bounds: PlayerShape
    val public placing: int option
    new(b, p) = { bounds = b; placing = p }

    member this.collisionBox with get() = BoundingPolygon(this.bounds)
    /// Player position, obtained from the bounding box
    member this.position with get() = this.bounds.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.bounds.Direction
  end


type MovingPlayerData =
  struct
    val public velocity: float
    val public turns: int
    val public lastTurnedLeft: bool
    val public currentTaunt: string option
    val public tauntTimer: int
    val public intersectingLines: bool list
    
    new(vel, turns, ltl, tnt, tntT, il) =
      { velocity = vel; turns = turns;
      lastTurnedLeft = ltl; currentTaunt = tnt; tauntTimer = tntT;
      intersectingLines = il }
    
    new(b: PlayerShape, vel, center: Vector2) =
      { velocity = vel; turns = if b.Center.Y >= center.Y then 0 else -1;
        // Always start on the opposite side
        lastTurnedLeft = b.Center.Y >= center.Y;
        currentTaunt = None; tauntTimer = 0;
        intersectingLines = [false; false; false; false] }
  end

/// Player state machine
type PlayerState =
  /// A player state that's still in the race
  | Moving of CommonPlayerData * MovingPlayerData
  | Crashed of CommonPlayerData

type Player = PlayerState