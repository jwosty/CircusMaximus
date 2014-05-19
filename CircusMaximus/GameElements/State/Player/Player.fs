namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision

type SpawnState = Spawning of int | Spawned
type MotionState = Moving of SpawnState * Velocity | Crashed of int
type FinishState = | Racing | Finished of Placing

type Player =
  { motionState: MotionState
    finishState: FinishState
    /// The player's number, e.g. player 1, player 2, etc
    number: int
    /// The player's color
    color: Color
    /// The word for the player's color
    colorString: string
    /// The number of frames that this player has existed for
    age: float
    /// Player bounds for collision
    bounds: PlayerShape
    /// Horse stats aggregated together
    horses: Horses
    /// The amount of turns this player has made so far
    turns: int
    /// Whether or not this player just came around the starting turn
    lastTurnedLeft: bool
    /// Whether or not this player is currently taunting
    tauntState: Taunt option
    /// The player's usable items
    items: Item list
    /// The index of the currently selected item
    selectedItem: int
    /// A list of active player effects
    effects: Effect list
    /// Particles attatched to this player (used for the taunt effect)
    particles: BoundParticle list
    /// For debugging; a list representing which lines on the player bounds are intersecting
    intersectingLines: bool list }
  
  member this.position = this.bounds.Center
  /// Player direction, in radians
  member this.direction = this.bounds.Direction
  member this.velocity = match this.motionState with | Moving(_, v) -> v | Crashed _ -> 0.
  member this.collisionBox = BoundingPolygon(this.bounds)
  member this.finished = match this.finishState with | Racing -> false | _ -> true
  
  /// The default number of players participating in the race (TODO: make all uses parameterized)
  static member numPlayers = 5
  /// The base player acceleration change in percent per frame
  static member baseAcceleration = 2.0
  /// A normal top speed, and the factor to convert a turn percentage into an absolute velocity
  static member baseTopSpeed = 8.0
  /// A normal turn speed, and the factor to convert a turn percentage into radians
  static member baseTurn = 0.03125  // 1/32nd radian
  
  static member unbalanceMidPoint = 0.5
  static member maxStatUnbalance = 0.1
  static member unbalanceTimes = 4
  
  static member crashDuration = 200
  static member spawnDuration = 100
  
  static member init horses (bounds: PlayerShape) number =
    let color, colorString = playerColorWithString number
    { motionState = Moving(Spawning Player.spawnDuration, 0.); finishState = Racing; tauntState = None
      number = number; color = color; colorString = colorString;
      items = List.init 11 (fun _ -> Item.SugarCubes)
      selectedItem = 0; age = 0.; bounds = bounds; horses = horses
      intersectingLines = [false; false; false; false; false; false]
      turns = if bounds.Center.Y >= Racetrack.center.Y then 0 else -1
      effects = []; particles = []
      lastTurnedLeft = bounds.Center.Y >= Racetrack.center.Y }