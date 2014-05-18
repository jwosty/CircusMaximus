namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Player =
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
    let finalΔdirection =
      // Being taunted affects players' turning ability
      match Effect.findLongest player.effects EffectType.Taunted with
      | Some _ -> Δdirection * 0.75
      | None -> Δdirection
    let finalDirection = player.direction + finalΔdirection
    
    positionForward player.position finalDirection player.velocity, finalDirection
  
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
  let basicNext (input: PlayerInput) (player: Player) respawnPoints collisionResults (racetrackCenter: Vector2) rand playerChariotSound =
    // Common code between crashed and moving players
    let tauntState = nextTauntState input.expectingTaunt rand player.tauntState
    let effects = Effect.nextEffects player.effects
    let particles = nextParticles rand player.particles player.effects
    
    match player.motionState with
    | Moving(spawnState, velocity) ->
      // If the player is colliding on the front, then the player is crashing
      match collisionResults with
        | true :: _ ->
          { player with motionState = Crashed Player.crashDuration }, Stopped
        | _ ->
          let spawnState =
            match spawnState with
            | Spawning safeTime -> Spawning (safeTime - 1)
            | Spawned -> Spawned
          
          let turn = -input.leftReignPull + input.rightReignPull
          let velocity =
            match Effect.findLongest player.effects EffectType.Sugar with
            | Some(_, EffectDurations.sugar) -> Player.baseTopSpeed * 2.5
            | _ ->
              // Accelerate if the player so wishes
              let v =
                if input.flickReigns
                then player.velocity + player.horses.acceleration
                else player.velocity
              // Slow down depending on how hard the reigns are being pulled
              let v =
                clampMin
                  (Player.baseTopSpeed * -0.25)
                  (v - (input.leftReignPull * input.rightReignPull / 2.0))
              // Slow down if the horses are going faster than they normally can
              if v > player.horses.topSpeed
                then clampMin player.horses.topSpeed (v - (player.horses.acceleration * 0.5))
                else v
          
          let position, direction = nextPositionDirection player turn
          let turns, lastTurnedLeft = nextTurns racetrackCenter input player position
          
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
    | Crashed timeUntilRespawn ->
      if timeUntilRespawn > 0 then
        { player with
            motionState = Crashed (timeUntilRespawn - 1)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
      else
        // Respawn the player
        let respawnPoint, respawnDirection =
          let rspI, rsp =
            respawnPoints
              |> List.mapi (fun i p -> i, p)
              |> List.minBy (fun (_, p) -> Vector2.DistanceSquared(player.position, p))
          let direction = respawnPoints.[List.wrapIndex respawnPoints (rspI - 1)] - rsp
          rsp, atan2 direction.Y direction.X |> float
        
        { player with
            motionState = Moving(Spawning 100, 0.4)
            bounds = new PlayerShape(respawnPoint, player.bounds.Width, player.bounds.Height, respawnDirection)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
  
  /// Updates a player like basicNext, but also handles input things
  let next fields (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) respawnPoints collisionResult playerChariotSound player =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player, playerChariotSound =
      if player.number = 1 then
        basicNext
          (PlayerInput.initFromKeyboard (lastKeyboard, keyboard) fields.settings)
          player respawnPoints collision Racetrack.center fields.rand playerChariotSound
      else
        let lastGamepad, gamepad = lastGamepads.[player.number - 2], gamepads.[player.number - 2]
        basicNext
          (PlayerInput.initFromGamepad (lastGamepad, gamepad) fields.settings)
          player respawnPoints collision Racetrack.center fields.rand playerChariotSound
    player, playerChariotSound