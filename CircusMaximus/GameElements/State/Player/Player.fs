namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision

/// A velocity, in percentage (where 0 = 0% and 1 = 100%) of a top speed
type Velocity = float
/// A player's finishing place in the race
type Placing = int
/// A length of time
type Duration = int
/// A taunt contains the string to display as well as the amount of time it lasts for
type Taunt = string * Duration

type MotionState = Moving of Velocity | Crashed
type FinishState = | Racing | Finished of Placing
type Effect = | Taunt

type Player =
  { motionState: MotionState
    finishState: FinishState
    /// The player's number, e.g. player 1, player 2, etc
    number: int
    /// The player's color
    color: Color
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
    effects: (Effect * Duration) list
    /// Particles attatched to this player (used for the taunt effect)
    particles: BoundParticle list
    /// For debugging; a list representing which lines on the player bounds are intersecting
    intersectingLines: bool list }
  
  member this.position = this.bounds.Center
  /// Player direction, in radians
  member this.direction = this.bounds.Direction
  member this.velocity = match this.motionState with | Moving v -> v | Crashed -> 0.
  member this.collisionBox = BoundingPolygon(this.bounds)
  member this.finished = match this.finishState with | Racing -> false | _ -> true

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Player =
  /// The number of players participating in the race
  let numPlayers = 5
  /// The amount of time a taunt lasts
  let tauntTime = 750
  
  /// The base player acceleration change in percent per frame
  let baseAcceleration = 0.005
  /// A normal top speed, and the factor to convert a turn percentage into an absolute velocity
  let baseTopSpeed = 5.
  /// A normal turn speed, and the factor to convert a turn percentage into an absolute velocity
  let baseTurn = 1.
  
  let unbalanceMidPoint = 0.5
  let maxStatUnbalance = 0.1
  let unbalanceTimes = 4
  
  let getColor = function
    | 1 -> Color.Red
    | 2 -> Color.Yellow
    | 3 -> Color.Green
    | 4 -> Color.Cyan
    | 5 -> Color.Blue
    | _ -> Color.White
  
  let init horses (bounds: PlayerShape) number =
    { motionState = Moving(0.); finishState = Racing; tauntState = None
      number = number; color = getColor number; items = [Item.SugarCubes]
      selectedItem = 0; age = 0.; bounds = bounds; horses = horses
      intersectingLines = [false; false; false; false]
      turns = if bounds.Center.Y >= Racetrack.center.Y then 0 else -1
      effects = []; particles = []
      lastTurnedLeft = bounds.Center.Y >= Racetrack.center.Y }
  
  let getBB (player: Player) = BoundingPolygon(player.bounds)
  
  /// Returns the new effects that the source player imposes on the destination player
  let applyEffects (source: Player) (destination: Player) =
    match source.tauntState with
    | Some(_, duration) when source <> destination ->
      if duration = tauntTime
      then [Effect.Taunt, tauntTime]    // Source player has just started taunting, so create one new effect
      else []                           // Source player has already been taunting, so nothing new
    | _ -> []                           // Source player isn't taunting, so nothing new
  
  let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
    if lastTurnedLeft && position.X > center.X then
      lastPosition.Y > center.Y && position.Y < center.Y
    elif (not lastTurnedLeft) && position.X < center.X then
      lastPosition.Y < center.Y && position.Y > center.Y
    else false
  
  let justFinished (oldPlayer: Player) (player: Player) = (not oldPlayer.finished) && player.finished
  
  /// Finds the effect that matches the given effect, using the one with the greatest remaining duration
  let findLongestEffect (effects: (Effect * Duration) list) key =
    let effects = List.filter (fun (e, _) -> e = key) effects
    match effects with
    | [] -> None
    | _ -> Some(List.maxBy snd effects)
  
  let nextEffects (effects: (Effect * Duration) list) =
    effects
      |> List.map (fun (e, d) -> e, d - 1)
      |> List.filter (fun (_, d) -> d > 0)
  
  /// Returns the next position and direction of the player and change in direction
  let nextPositionDirection (player: Player) Δdirection =
    let Δdirection = Δdirection * player.horses.turn
    let finalVelocity = player.velocity * player.horses.topSpeed
    let finalΔdirection =
      // Taunting affects players' turning ability
      match findLongestEffect player.effects Effect.Taunt with
      | Some _ -> Δdirection * 0.75
      | None -> Δdirection
    let finalDirection = player.direction + finalΔdirection
    
    positionForward player.position finalDirection finalVelocity, finalDirection
  
  /// Returns the next number of laps and whether or not the player last turned on the left side of the map
  let nextTurns racetrackCenter (input: PlayerInput) (player: Player) nextPosition =
    if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition || input.advanceLap then
      player.turns + 1, not player.lastTurnedLeft
    else
      player.turns, player.lastTurnedLeft
  
  /// Functionally updates a player's state, finishing the player if needed and returning the new player and placing
  let nextPlayerFinish maxTurns lastPlacing (player: Player) =
    match player.finishState with
    | Racing ->
      if player.turns >= maxTurns
      then { player with finishState = Finished(lastPlacing + 1) }, lastPlacing + 1
      else player, lastPlacing
    | Finished(placing) -> player, lastPlacing
  
  /// Returns a new taunt if needed, otherwise none
  let nextTauntState expectingTaunt rand = function
    | Some(taunt, tauntTimer) ->
      if tauntTimer >= 0 then
        Some(taunt, tauntTimer - 1)
      else
        None
    | None ->
      if expectingTaunt
      then Some(Taunt.pickTaunt rand, tauntTime)
      else None
  
  /// A basic function an updated version of the given player model. Players are not given a placing here.
  let basicNext (input: PlayerInput) (player: Player) collisionResults (racetrackCenter: Vector2) rand playerChariotSound =
    // Common code between crashed and moving players
    let tauntState = nextTauntState input.expectingTaunt rand player.tauntState
    let effects = nextEffects player.effects
    let e = findLongestEffect player.effects Effect.Taunt
    let particles =
      match e with
      | Some(effect, duration) ->
        let factor = (float duration) / (float tauntTime)
        player.particles @ BoundParticle.RandBatchInit rand factor
      | None -> player.particles
        // Update particles
      |> List.map BoundParticle.nextParticle
        // Delete old particles
      |> List.filter (fun p -> p.age < BoundParticle.particleAge)
    
    match player.motionState with
    | Moving velocity ->
      // If the player is colliding on the front, then the player is crashing
      match collisionResults with
        | true :: _ ->
          { player with motionState = Crashed }, Stopped
        | _ ->
          let position, direction = nextPositionDirection player input.turn
          let turns, lastTurnedLeft = nextTurns racetrackCenter input player position
          let velocity =
            if player.velocity > input.power
              then clampMin (player.velocity - player.horses.acceleration) input.power
            elif player.velocity < input.power
              then clampMax (player.velocity + player.horses.acceleration) input.power
            else player.velocity
          { player with
              motionState = Moving(velocity)
              bounds = new PlayerShape(position, player.bounds.Width, player.bounds.Height, direction)
              age = player.age + 1.; turns = turns; lastTurnedLeft = lastTurnedLeft
              tauntState = tauntState; effects = effects; particles = particles; intersectingLines = collisionResults },
            if player.velocity >= 0.75
            then Looping
            else Paused
    | Crashed ->
      { player with
          tauntState = tauntState;
          effects = effects;
          particles = particles }, playerChariotSound
  
  let findByNumber number players = List.find (fun (player: Player) -> player.number = number) players
  
  /// Updates a player like basicNext, but also handles input things
  let next (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) rand collisionResult playerChariotSound player =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player, playerChariotSound =
      if player.number = 1 then
        basicNext
          (PlayerInput.initFromKeyboard (lastKeyboard, keyboard))
          player collision Racetrack.center rand playerChariotSound
      else
        let lastGamepad, gamepad = lastGamepads.[player.number - 2], gamepads.[player.number - 2]
        basicNext
          (PlayerInput.initFromGamepad (lastGamepad, gamepad))
          player collision Racetrack.center rand playerChariotSound
    player, playerChariotSound