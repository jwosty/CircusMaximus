namespace CircusMaximus.State.Player
open System
open Microsoft.Xna.Framework
open CircusMaximus

/// The moving player state, that is, one who is still in the race
type Moving =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public boundingBox: BoundingBox2D.BoundingBox2D
    val public velocity: float
    val public turns: int
    val public lastTurnedLeft: bool
    val public currentTaunt: string option
    val public tauntTimer: int
    val public intersectingLines: bool list
    
    new(bb, vel, turns, ltl, tnt, tntT, il) =
      { boundingBox = bb; velocity = vel; turns = turns;
      lastTurnedLeft = ltl; currentTaunt = tnt; tauntTimer = tntT;
      intersectingLines = il }
    
    new(bb, vel, center: Vector2) =
      { boundingBox = bb; velocity = vel;
        turns = if bb.Center.Y >= center.Y then 0 else -1;
        // Always start on the opposite side
        lastTurnedLeft = bb.Center.X >= center.X;
        currentTaunt = None; tauntTimer = 0;
        intersectingLines = [] }
    
    /// Player position, obtained from the bounding box
    member this.position with get() = this.boundingBox.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.boundingBox.Direction
  end

/// The Crashed player state, which occurs when a player, well, crashes
type Crashed =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public boundingBox: BoundingBox2D.BoundingBox2D
    new(bb) = { boundingBox = bb }
    /// Player position, obtained from the bounding box
    member this.position with get() = this.boundingBox.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.boundingBox.Direction
  end