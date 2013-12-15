/// Contains functions and constants operating on players
module CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open CircusMaximus
open CircusMaximus.State
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision

let tauntTime = 1000

let getBB (player: Player) = BoundingPolygon(player.bounds)

/// Returns a list of effects that the source player imposes on the destination player
let applyEffects (source: Player) (destination: Player) =
  match source.tauntState with
  | Some(_, _) when source <> destination -> [Effect.Taunt, tauntTime]
  | _ -> []

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

let positionForward position direction distance = position + (cos direction * distance @@ sin direction * distance)

/// Returns the next position and direction of the player and change in direction
#nowarn "49"
let nextPositionDirection (player: Player) Δdirection =
  let Δdirection =
    // Taunting affects players' turning ability
    match findLongestEffect player.effects Effect.Taunt with
    | Some _ -> Δdirection * 0.75
    | None -> Δdirection
  positionForward player.position player.direction player.velocity, player.direction + Δdirection

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let nextLaps racetrackCenter (input: PlayerInputState) (player: Player) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition || input.advanceLap then
    player.turns + 1, not player.lastTurnedLeft
  else
    player.turns, player.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let nextTauntState expectingTaunt rand = function
  | Some(taunt, tauntTimer) ->
    if tauntTimer >= 0 then
      Some(taunt, tauntTimer - 1)
    else
      None
  | None ->
    if expectingTaunt then
      Some(Taunt.pickTaunt rand, tauntTime)
    else
      None

let updateParticle particle =
  { particle with
      position = positionForward particle.position particle.direction (cos(particle.age / 64.))
      direction = particle.direction
      age = particle.age + 1. }

/// Returns an updated version of the given player model. Players are not given a placing here.
let next (input: PlayerInputState) (player: Player) playerIndex collisionResults expectingTaunt (racetrackCenter: Vector2) rand (assets: GameContent) =
  // Common code between crashed and moving players
  let tauntState = nextTauntState expectingTaunt rand player.tauntState
  let effects = nextEffects player.effects
  let particles =
    player.particles
      // Add some particles if necessary
      |> List.appendFrontIf
        (findLongestEffect player.effects Effect.Taunt |> isSome)
        (BoundParticle.RandBatchInit ...<| rand)
      // Update particles
      |> List.map updateParticle
      // Delete old particles
      |> List.filter (fun p -> p.age < 100.53)
  
  match player.motionState with
  | Moving velocity ->
    let snd = assets.ChariotSound.[playerIndex]
    // If the player is colliding on the front, then the player is crashing
    match collisionResults with
      | true :: _ ->
        snd.Stop()
        { player with motionState = Crashed }
      | _ ->
        if (player.velocity >= 3.) && (snd.State <> SoundState.Playing) then
          snd.Resume()
        if (player.velocity < 3.) && (snd.State = SoundState.Playing) then
          snd.Pause()
        
        let position, direction = nextPositionDirection player input.turn
        let turns, lastTurnedLeft = nextLaps racetrackCenter input player position

        { motionState = Moving(((player.velocity * 128.) + input.power) / 129.0); finishState = player.finishState
          bounds = new PlayerShape(position, player.bounds.Width, player.bounds.Height, direction)
          index = player.index; turns = turns; age = player.age + 1.; lastTurnedLeft = lastTurnedLeft
          tauntState = tauntState; effects = effects; particles = particles; intersectingLines = collisionResults }
  | Crashed ->
    { player with
        tauntState = tauntState;
        effects = effects;
        particles = particles }