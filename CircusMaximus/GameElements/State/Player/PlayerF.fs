﻿namespace CircusMaximus.Functions
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
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

#nowarn "49"
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Player =
  let getBB (player: Player) = BoundingPolygon(player.bounds)
  
  /// Returns the new effects that the source player imposes on the destination player
  let applyEffects (source: Player) (destination: Player) =
    match source.tauntState with
    | Some(_, duration) when source <> destination ->
      if duration = EffectDurations.taunt
      then (TurnVelocityDecreased, EffectDurations.taunt) :: (if source.ability = TauntSlow then [VelocityDecreased, EffectDurations.taunt] else [])
      else []
    | _ -> []
  
  let isPassingTurnLine (center: Vector2<px>) lastTurnedLeft (lastPosition: Vector2<px>) (position: Vector2<px>) =
    if lastTurnedLeft && position.X > center.X then
      lastPosition.Y > center.Y && position.Y < center.Y
    elif (not lastTurnedLeft) && position.X < center.X then
      lastPosition.Y < center.Y && position.Y > center.Y
    else false
  
  let justFinished (oldPlayer: Player) (player: Player) = (not oldPlayer.finished) && player.finished
  
  /// Finds a player by their number
  let findByNumber number players = List.find (fun (player: Player) -> player.number = number) players
  
  /// Returns the next position and direction of the player and change in direction
  let nextPositionDirection (player: Player) (Δdirection: float<r>) deltaTime =
    let Δdirection = Δdirection * player.horses.turn
    let finalΔdirection =
      // Being taunted affects players' turning ability
      match Effect.findLongest player.effects EffectType.TurnVelocityDecreased with
      | Some _ -> Δdirection * 0.75
      | None -> Δdirection
    let finalDirection = player.direction + finalΔdirection
    let finalVelocity = (if List.exists (fst >> ((=) VelocityDecreased)) player.effects then player.velocity * 0.5 else player.velocity)
    positionForward player.position finalDirection (finalVelocity * deltaTime), finalDirection
  
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
    match Effect.findLongest effects EffectType.TurnVelocityDecreased with
      // Player is being taunted, so randomly generate particles
    | Some(effect, duration) ->
      let factor = (float duration) / (float EffectDurations.taunt)
      particles @ BoundParticle.RandBatchInit rand factor 0.5
      // Player is not being taunted, so don't generate any more particles
    | None -> particles
    |> List.map BoundParticle.nextParticle
    |> List.filter (fun p -> p.age < BoundParticle.particleAge) // Update particles, culling the old ones
  
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
  let basicNext (input: PlayerInput) (player: Player) respawnPoints collisionResults (racetrackCenter: Vector2<px>) (deltaTime: float<s>) rand playerChariotSound =
    // Common code between crashed and moving players
    let tauntState = nextTauntState input.expectingTaunt rand player.tauntState
    let effects = Effect.nextEffects player.effects
    let particles = nextParticles rand player.particles player.effects
    
    match player.motionState with
    | Moving(spawnState, velocity, accelerationTimer) ->
      // If the player is colliding on the front, then the player is crashing
      match collisionResults with
        | true :: _ ->
          { player with motionState = Crashed Player.crashDuration }, Stopped
        | _ ->
          let spawnState =
            match spawnState with
            | Spawning safeTime -> Spawning (safeTime - deltaTime)
            | Spawned -> Spawned
          
          let turn = (-input.leftReignPull + input.rightReignPull) * 1.<r>
          let accelerationTimer = if input.flickReigns then Player.accelerationDuration else max 0.<s> (accelerationTimer - deltaTime)
          let velocity =
            match Effect.findLongest player.effects VelocityIncreased with
            | Some(_, EffectDurations.sugar) -> Player.baseTopSpeed
            | _ ->
              // Accelerate if the player so wishes
              let v =
                if accelerationTimer > 0.<s>
                then player.velocity + (player.horses.acceleration * deltaTime)
                else player.velocity
              // Slow down depending on how hard the reigns are being pulled
              let v =
                clampMin
                  (Player.baseTopSpeed * -0.25)
                  (v - (input.leftReignPull * input.rightReignPull * 1.<px> / (2. * deltaTime)))
              // Slow down if the horses are going faster than they normally can
              if v > player.horses.topSpeed
                then clampMin player.horses.topSpeed (v - (player.horses.acceleration * deltaTime))
                else v
          
          let position, direction = nextPositionDirection player turn deltaTime
          let turns, lastTurnedLeft = nextTurns racetrackCenter input player position
          
          let selectedItem = MathHelper.Clamp(player.selectedItem + input.selectorΔ, 0, player.items.Length - 1)
          let items, effects =
            if input.isUsingItem && player.items.Length > 0
              then useItem player.items effects selectedItem
              else player.items, effects
          
          { player with
              motionState = Moving(spawnState, velocity, accelerationTimer)
              bounds = new PlayerShape(position, player.bounds.Dimensions, direction)
              age = player.age + deltaTime; selectedItem = selectedItem; turns = turns; lastTurnedLeft = lastTurnedLeft
              tauntState = tauntState; effects = effects; items = items; particles = particles; intersectingLines = collisionResults },
            if player.velocity >= 50.0<px/s>
            then Looping
            else Paused
    | Crashed timeUntilRespawn ->
      if timeUntilRespawn > 0.<s> then
        { player with
            motionState = Crashed (timeUntilRespawn - deltaTime)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
      else
        // Respawn the player
        let respawnPoint, respawnDirection =
          let rspI, rsp =
            respawnPoints
              |> List.mapi (fun i p -> i, p)
              |> List.minBy (fun (_, p) -> (player.position - p).LengthSquared)
          let direction = respawnPoints.[List.wrapIndex respawnPoints (rspI - 1)] - rsp
          rsp, atan2 (float direction.Y) (float direction.X)
        
        { player with
            motionState = Moving(Spawning Player.spawnDuration, 0.006<px/s>, 0.<s>)
            bounds = new PlayerShape(respawnPoint, player.bounds.Dimensions, respawnDirection)
            tauntState = tauntState
            effects = effects
            particles = particles }, playerChariotSound
  
  /// Updates a player like basicNext, but also handles input things
  let next fields input respawnPoints deltaTime collisionResult playerChariotSound player =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player, playerChariotSound =
      if player.number = 1 then
        basicNext
          (PlayerInput.initFromKeyboard (input.lastKeyboard, input.keyboard) fields.settings)
          player respawnPoints collision Racetrack.center deltaTime fields.rand playerChariotSound
      else
        let lastGamepad, gamepad = input.lastGamepads.[player.number - 2], input.gamepads.[player.number - 2]
        basicNext
          (PlayerInput.initFromGamepad (lastGamepad, gamepad) fields.settings)
          player respawnPoints collision Racetrack.center deltaTime fields.rand playerChariotSound
    player, playerChariotSound