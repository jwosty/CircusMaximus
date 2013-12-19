namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open CircusMaximus
open CircusMaximus.State
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision

type Velocity = float
type Placing = int
type Duration = int
type Taunt = string * Duration

type MotionState = Moving of Velocity | Crashed
type FinishState = | Racing | Finished of Placing
type Effect = | Taunt

type Player =
  { motionState: MotionState
    finishState: FinishState
    bounds: PlayerShape
    index: int
    age: float
    turns: int
    lastTurnedLeft: bool
    tauntState: Taunt option
    effects: (Effect * Duration) list
    particles: BoundParticle list
    intersectingLines: bool list }

  member this.position = this.bounds.Center
  /// Player direction, in radians
  member this.direction = this.bounds.Direction
  member this.velocity = match this.motionState with | Moving v -> v | Crashed -> 0.
  member this.collisionBox = BoundingPolygon(this.bounds)
  member this.finished = match this.finishState with | Racing -> false | _ -> true

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Player =
  let numPlayers = 5
  let tauntTime = 750

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

  /// Returns an updated version of the given player model. Players are not given a placing here.
  let next (input: PlayerInputState) (player: Player) playerIndex collisionResults expectingTaunt (racetrackCenter: Vector2) rand (assets: GameContent) =
    // Common code between crashed and moving players
    let tauntState = nextTauntState expectingTaunt rand player.tauntState
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