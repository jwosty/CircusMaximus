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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Player =
  /// The number of players participating in the race
  let numPlayers = 5
  
  /// The base player acceleration change in percent per frame
  let baseAcceleration = 0.05
  /// A normal top speed, and the factor to convert a turn percentage into an absolute velocity
  let baseTopSpeed = 8.
  /// A normal turn speed, and the factor to convert a turn percentage into degrees
  let baseTurn = 1.5
  
  let unbalanceMidPoint = 0.5
  let maxStatUnbalance = 0.1
  let unbalanceTimes = 4
  
  let crashDuration = 200
  
  let getColor = function
    | 1 -> Color.Red
    | 2 -> Color.Yellow
    | 3 -> Color.Green
    | 4 -> Color.Cyan
    | 5 -> Color.Blue
    | _ -> Color.White
  
  let init horses (bounds: PlayerShape) number =
    { motionState = Moving(Spawning 100, 0.); finishState = Racing; tauntState = None
      number = number; color = getColor number; items = List.init 11 (fun _ -> Item.SugarCubes)
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
      if duration = EffectDurations.taunt
      then [EffectType.Taunted, EffectDurations.taunt]  // Source player has just started taunting, so create one new effect
      else []                                           // Source player has already been taunting, so nothing new
    | _ -> []                                           // Source player isn't taunting, so nothing new
  
  let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
    if lastTurnedLeft && position.X > center.X then
      lastPosition.Y > center.Y && position.Y < center.Y
    elif (not lastTurnedLeft) && position.X < center.X then
      lastPosition.Y < center.Y && position.Y > center.Y
    else false
  
  let justFinished (oldPlayer: Player) (player: Player) = (not oldPlayer.finished) && player.finished
  
  /// Finds a player by their number
  let findByNumber number players = List.find (fun (player: Player) -> player.number = number) players
  
  /// Returns the next position and direction of the player and change in direction
  let nextPositionDirection (player: Player) Δdirection =
    let Δdirection = Δdirection * player.horses.turn
    let velocity = player.velocity
    let finalΔdirection =
      // Being taunted affects players' turning ability
      match Effect.findLongest player.effects EffectType.Taunted with
      | Some _ -> Δdirection * 0.75
      | None -> Δdirection
    let finalDirection = player.direction + finalΔdirection
    
    positionForward player.position finalDirection velocity, finalDirection
  
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
      then Some(Taunt.pickTaunt rand, EffectDurations.taunt)
      else None
  
  /// Updates/adds/destroys player particles
  let nextParticles rand particles effects =
    match Effect.findLongest effects EffectType.Taunted with
      // Player is being taunted, so randomly generate particles
    | Some(effect, duration) ->
      let factor = (float duration) / (float EffectDurations.taunt)
      particles @ BoundParticle.RandBatchInit rand factor
      // Player is not being taunted, so don't generate any more particles
    | None -> particles
      // Update particles
    |> List.map BoundParticle.nextParticle
      // Delete old particles
    |> List.filter (fun p -> p.age < BoundParticle.particleAge)
  
  /// Uses the given item, deleting the item and adding the appropriate effect
  let useItem items effects itemIndex =
    let rec useItem items (effects: Effect list) i itemIndex =
      if itemIndex = i then
        List.tail items, (items |> List.head |> Item.getEffect) :: effects
      else
        let updatedItems, updatedEffects = useItem (List.tail items) effects (i + 1) itemIndex
        List.head items :: updatedItems, updatedEffects
    
    useItem items effects 0 itemIndex
  
  /// A basic function an updated version of the given player model. Players are not given a placing here.
  let basicNext (input: PlayerInput) (player: Player) (racetrack: RacetrackSpinaShape) collisionResults (racetrackCenter: Vector2) rand playerChariotSound =
    // Common code between crashed and moving players
    let tauntState = nextTauntState input.expectingTaunt rand player.tauntState
    let effects = Effect.nextEffects player.effects
    let particles = nextParticles rand player.particles player.effects
    
    match player.motionState with
    | Moving(spawnState, velocity) ->
      // If the player is colliding on the front, then the player is crashing
      match collisionResults with
        | true :: _ ->
          { player with motionState = Crashed 0 }, Stopped
        | _ ->
          let spawnState =
            match spawnState with
            | Spawning safeTime -> Spawning (safeTime - 1)
            | Spawned -> Spawned
          let position, direction = nextPositionDirection player input.turn
          let turns, lastTurnedLeft = nextTurns racetrackCenter input player position
          let baseVelocity =
            match Effect.findLongest player.effects EffectType.Sugar with
            | Some(_, EffectDurations.sugar) -> 10.0
            | _ -> player.velocity
          let absPower = input.power * player.horses.topSpeed
          let velocity =
            if baseVelocity > absPower
              then clampMin 0.0 (baseVelocity - (player.horses.acceleration * 2.0))
            elif baseVelocity < absPower
              then baseVelocity + player.horses.acceleration
            else baseVelocity
          let selectedItem = MathHelper.Clamp(player.selectedItem + input.selectorΔ, 0, player.items.Length - 1)
          let items, effects =
            if input.isUsingItem && player.items.Length > 0
              then useItem player.items effects selectedItem
              else player.items, effects
          { player with
              motionState = Moving(spawnState, velocity)
              bounds = new PlayerShape(position, player.bounds.Width, player.bounds.Height, direction)
              age = player.age + 1.; selectedItem = selectedItem; turns = turns; lastTurnedLeft = lastTurnedLeft
              tauntState = tauntState; effects = effects; items = items; particles = particles; intersectingLines = collisionResults },
            if player.velocity >= 0.75
            then Looping
            else Paused
    | Crashed timeCrashed ->
      if timeCrashed < crashDuration then
        { player with
            motionState = Crashed (timeCrashed + 1)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
      else
        // Respawn the player
        let respawnPoint, respawnDirection =
          let rspPoints = racetrack.RespawnPath
          let rspI, rsp =
            rspPoints
              |> List.mapi (fun i p -> i, p)
              |> List.minBy (fun (_, p) -> Vector2.DistanceSquared(player.position, p))
          let direction = rspPoints.[List.wrapIndex rspPoints (rspI - 1)] - rsp
          rsp, atan2 direction.Y direction.X |> float
          
        { player with
            motionState = Moving(Spawning 100, 0.4)
            bounds = new PlayerShape(respawnPoint, player.bounds.Width, player.bounds.Height, respawnDirection)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
  
  /// Updates a player like basicNext, but also handles input things
  let next (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) rand settings racetrack collisionResult playerChariotSound player =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player, playerChariotSound =
      if player.number = 1 then
        basicNext
          (PlayerInput.initFromKeyboard (lastKeyboard, keyboard) settings)
          player racetrack collision Racetrack.center rand playerChariotSound
      else
        let lastGamepad, gamepad = lastGamepads.[player.number - 2], gamepads.[player.number - 2]
        basicNext
          (PlayerInput.initFromGamepad (lastGamepad, gamepad) settings)
          player racetrack collision Racetrack.center rand playerChariotSound
    player, playerChariotSound