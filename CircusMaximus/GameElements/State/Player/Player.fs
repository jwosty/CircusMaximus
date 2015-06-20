namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Collision
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Types.UnitSymbols

type SpawnState = Spawning of float<s> | Spawned
type MotionState =
  | Moving of
      SpawnState *
      velocity:float<px/s> *
      /// How much longer the player should continue accelerating
      accelerationTimer:float<s>
  | Crashed of float<s>
type FinishState = | Racing | Finished of Placing

type Player =
  { motionState: MotionState
    finishState: FinishState
    /// This player's number, e.g. player 1, player 2, etc
    number: int
    /// This player's color
    color: Color
    /// The word for this player's color
    colorString: string
    /// The player's unique ability
    ability: Ability
    /// The number of seconds that this player has existed for
    age: float<s>
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
  member this.velocity = match this.motionState with | Moving(_, velocity, _) -> velocity | Crashed _ -> 0.<px/s>
  member this.accelerationTimer = match this.motionState with | Moving(_, _, acceleration) -> acceleration | Crashed _ -> 0.<s>
  member this.collisionBox = BoundingPolygon(this.bounds)
  member this.finished = match this.finishState with | Racing -> false | _ -> true
  
  /// The default number of players participating in the race (TODO: make all uses parameterized)
  static member numPlayers = 5
  /// The base player acceleration change in percent per frame
  static member baseAcceleration = 100.<px/s^2>
  /// A normal top speed, and the factor to convert a turn percentage into an absolute velocity
  static member baseTopSpeed = 500.<px/s>
  /// A normal turn speed, and the factor to convert a turn percentage into radians
  static member baseTurn = 0.03125  // 1/32nd radian
  /// Duration of time players accelerate when they flick the reins (so its not a single frame action)
  static member accelerationDuration = 1.<s>
  
  static member unbalanceMidPoint = 0.5
  static member maxStatUnbalance = 0.1
  static member unbalanceTimes = 4
  
  static member crashDuration = 3.<s>
  static member spawnDuration = 1.25<s>
  
  static member init horses (bounds: PlayerShape) number =
    let color, colorString = playerColorWithString number
    { motionState = Moving(Spawning Player.spawnDuration, 0.<px/s>, 0.<s>); finishState = Racing;
      tauntState = None; ability = TauntSlow
      number = number; color = color; colorString = colorString;
      items = List.init 11 (fun _ -> Item.SugarCubes)
      selectedItem = 0; age = 0.<s>; bounds = bounds; horses = horses
      intersectingLines = [false; false; false; false; false; false]
      turns = if bounds.Center.Y >= Racetrack.center.Y then 0 else -1
      effects = []; particles = []
      lastTurnedLeft = bounds.Center.Y >= Racetrack.center.Y }