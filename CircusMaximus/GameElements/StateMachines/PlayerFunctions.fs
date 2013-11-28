/// Contains functions and constants operating on players
module CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open CircusMaximus
open CircusMaximus.State
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Collision

let tauntTime = 1000

let getBB (player: Player) = BoundingPolygon(player.bounds)

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

let justFinished (oldPlayer: Player) (player: Player) = (not oldPlayer.finished) && player.finished

/// Returns the next position and direction of the player and change in direction
#nowarn "49"
let nextPositionDirection (player: Player) Δdirection =
  (player.position
    + (   cos player.direction * player.velocity
       @@ sin player.direction * player.velocity),
   player.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let nextLaps racetrackCenter (input: PlayerInputState) (player: Player) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition || input.advanceLap then
    player.turns + 1, not player.lastTurnedLeft
  else
    player.turns, player.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let nextTauntState expectingTaunt = function
  | Some(taunt, tauntTimer) ->
    if tauntTimer >= 0 then
      Some(taunt, tauntTimer - 1)
    else
      None
  | None ->
    if expectingTaunt then
      Some(Taunt.pickTaunt (), tauntTime)
    else
      None

/// Returns an updated version of the given player model. Players are not given a placing here.
let next (input: PlayerInputState) (player: Player) playerIndex collisionResults expectingTaunt (racetrackCenter: Vector2) (assets: GameContent) =
  match player.motionState with
  | Moving velocity ->
    // If the player is colliding on the front, then the player is crashing
    match collisionResults with
      | true :: _ -> { player with motionState = Crashed }
      | _ ->
        let snd = assets.ChariotSound.[playerIndex]
        if (player.velocity >= 3.) && (snd.State <> SoundState.Playing) then
          snd.Resume()
        if (player.velocity < 3.) && (snd.State = SoundState.Playing) then
          snd.Pause()
        let position, direction = nextPositionDirection player input.turn
        let turns, lastTurnedLeft = nextLaps racetrackCenter input player position
        let tauntState = nextTauntState expectingTaunt player.tauntState
        { motionState = Moving(((player.velocity * 128.) + input.power) / 129.0); finishState = player.finishState
          bounds = new PlayerShape(position, player.bounds.Width, player.bounds.Height, direction)
          index = player.index; turns = turns; lastTurnedLeft = lastTurnedLeft
          tauntState = tauntState; intersectingLines = collisionResults }
  | Crashed -> player