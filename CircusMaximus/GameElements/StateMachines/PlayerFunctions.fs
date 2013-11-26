/// Contains functions and constants pertaining to players
[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module CircusMaximus.Player
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open CircusMaximus
open CircusMaximus.Player
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input

let tauntTime = 1000

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

let commonPlayerData = function
  | Player.Moving(player, _) -> player
  | Player.Crashed player -> player

let playerBB = function
  | Player.Moving(player, _) -> player.collisionBox
  | Player.Crashed player -> player.collisionBox

let playerPlacing = function
  | Player.Moving(player, _) -> player.placing
  | Player.Crashed player -> player.placing

/// Returns the next position and direction of the player and change in direction
#nowarn "49"
let nextPositionDirection (commonData: CommonPlayerData, movingData: MovingPlayerData) Δdirection =
  (commonData.position
    + (   cos commonData.direction * movingData.velocity
       @@ sin commonData.direction * movingData.velocity),
   commonData.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let nextLaps racetrackCenter (input: PlayerInputState) (commonData: CommonPlayerData, movingData: MovingPlayerData) nextPosition =
  if isPassingTurnLine racetrackCenter movingData.lastTurnedLeft commonData.position nextPosition || input.advanceLap then
    movingData.turns + 1, not movingData.lastTurnedLeft
  else
    movingData.turns, movingData.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let nextTauntState (player: MovingPlayerData) expectingTaunt =
  if player.tauntTimer > 0 then
    player.currentTaunt, player.tauntTimer - 1
  elif expectingTaunt then
    Some(Taunt.pickTaunt ()), tauntTime
  else
    None, player.tauntTimer - 1

let nextMoving (input: PlayerInputState) (commonData: CommonPlayerData, movingData: MovingPlayerData) playerIndex collisionResults lastPlacing expectingTaunt racetrackCenter (assets: GameContent) =
  let snd = assets.ChariotSound.[playerIndex]
  // Warning: mutable code; a necessary evil here...
  if movingData.velocity >= 3.0 then
    if not (snd.State = SoundState.Playing) then
      snd.Resume()
  elif movingData.velocity < 3.0 then
    if snd.State = SoundState.Playing then
      snd.Pause()
  let position, direction = nextPositionDirection (commonData, movingData) input.turn
  // If the player has crossed the threshhold not more than once in a row, increment the turn count
  let turns, lastTurnedLeft = nextLaps racetrackCenter input (commonData, movingData) position
  let taunt, tauntTimer = nextTauntState movingData expectingTaunt
  let placing, nextPlacing =
    match commonData.placing with
    | Some _ -> commonData.placing, None
    | None ->
      if turns >= 13 then
        if lastPlacing < 3 then assets.CrowdCheerSound.Play() |> ignore // make the crowd cheer to congradulate the player for finishing in the top 3
        twice(Some(lastPlacing + 1))
      else twice(None)
  
  Player.Moving(
    new CommonPlayerData(new PlayerShape(position, commonData.bounds.Width, commonData.bounds.Height, direction), placing),
    new MovingPlayerData(((movingData.velocity * 128.0) + input.power) / 129.0, turns, lastTurnedLeft, taunt, tauntTimer, collisionResults))

/// Update the player with the given parameters, but this is functional, so it won't actually modify anything
let nextPlayer (input: PlayerInputState) player playerIndex collisionResults lastPlacing expectingTaunt (racetrackCenter: Vector2) (assets: GameContent) =
  match player with
  | Player.Moving(commonData, movingData) ->
    // If the player is colliding on the front, then the player is crashing
    match collisionResults with
      | true :: _ ->
        assets.ChariotSound.[playerIndex].Stop()
        Player.Crashed(new CommonPlayerData(commonData.bounds, commonData.placing))
      | _ -> nextMoving input (commonData, movingData) playerIndex collisionResults lastPlacing expectingTaunt racetrackCenter assets
  | Crashed _ -> player