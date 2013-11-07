namespace CircusMaximus.State.Player
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

/// The moving player state, that is, one who is still in the race
type Moving =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public boundingBox: OrientedRectangle
    val public velocity: float
    val public turns: int
    val public lastTurnedLeft: bool
    val public currentTaunt: string option
    val public tauntTimer: int
    val public intersectingLines: bool list
    val public placing: int option
    
    new(bb, vel, turns, ltl, tnt, tntT, il, p) =
      { boundingBox = bb; velocity = vel; turns = turns;
      lastTurnedLeft = ltl; currentTaunt = tnt; tauntTimer = tntT;
      intersectingLines = il; placing = p }
    
    new(bb, vel, center: Vector2, p) =
      { boundingBox = bb; velocity = vel;
        turns = if bb.Center.Y >= center.Y then 0 else -1;
        // Always start on the opposite side
        lastTurnedLeft = bb.Center.Y >= center.Y;
        currentTaunt = None; tauntTimer = 0;
        intersectingLines = [false; false; false; false]; placing = p }
    
    member this.collisionBox with get() = BoundingPolygon(this.boundingBox)
    /// Player position, obtained from the bounding box
    member this.position with get() = this.boundingBox.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.boundingBox.Direction
  end

/// The Crashed player state, which occurs when a player, well, crashes
type Crashed =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public boundingBox: OrientedRectangle
    val public placing: int option
    new(bb, p) = { boundingBox = bb; placing = p }
    
    member this.collisionBox with get() = BoundingPolygon(this.boundingBox)
    /// Player position, obtained from the bounding box
    member this.position with get() = this.boundingBox.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.boundingBox.Direction
  end