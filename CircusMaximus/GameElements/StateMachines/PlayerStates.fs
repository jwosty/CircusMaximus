namespace CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

type Velocity = float
type Placing = int
type Taunt = string * int

type MotionState = Moving of Velocity | Crashed
type FinishState = | Racing | Finished of Placing

type Player =
  { motionState: MotionState
    finishState: FinishState
    bounds: PlayerShape
    turns: int
    lastTurnedLeft: bool
    tauntState: Taunt option
    intersectingLines: bool list }

  member this.position = this.bounds.Center
  /// Player direction, in radians
  member this.direction = this.bounds.Direction
  member this.velocity = match this.motionState with | Moving v -> v | Crashed -> 0.
  member this.collisionBox = BoundingPolygon(this.bounds)
  member this.finished = match this.finishState with | Racing -> false | _ -> true