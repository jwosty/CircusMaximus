/// Contains functions and constants pertaining to players
[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module CircusMaximus.Player
// =====================
// == XNA INDEPENDENT ==
// =====================
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

let playerBB = function
  | Player.Moving player -> player.collisionBox
  | Player.Crashed player -> player.collisionBox

let playerPlacing = function
  | Player.Moving player -> player.placing
  | Player.Crashed player -> player.placing

/// Returns the next position and direction of the player and change in direction
#nowarn "49"
let nextPositionDirection (player: MovingData) Δdirection =
  (player.position
    + (   cos player.direction * player.velocity
       @@ sin player.direction * player.velocity),
   player.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let nextLaps racetrackCenter (input: PlayerInputState) (player: MovingData) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition || input.advanceLap then
    player.turns + 1, not player.lastTurnedLeft
  else
    player.turns, player.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let nextTauntState (player: MovingData) expectingTaunt =
  if player.tauntTimer > 0 then
    player.currentTaunt, player.tauntTimer - 1
  elif expectingTaunt then
    Some(Taunt.pickTaunt ()), tauntTime
  else
    None, player.tauntTimer - 1

/// Update the player with the given parameters, but this is functional, so it won't actually modify anything
let nextPlayer (input: PlayerInputState) player playerIndex collisionResults lastPlacing expectingTaunt (racetrackCenter: Vector2) (assets: GameContent) =
  match player with
  | Player.Moving player ->
    let snd = assets.ChariotSound.[playerIndex]
    // Warning: mutable code; a necessary evil here...
    if player.velocity >= 3.0 then
      if not (snd.State = SoundState.Playing) then
        snd.Resume()
    elif player.velocity < 3.0 then
      if snd.State = SoundState.Playing then
        snd.Pause()
    //let movingPlayers = filterMoving otherPlayers
    let position, direction = nextPositionDirection player input.turn
    // If the player has crossed the threshhold not more than once in a row, increment the turn count
    let turns, lastTurnedLeft = nextLaps racetrackCenter input player position
    let taunt, tauntTimer = nextTauntState player expectingTaunt
    let placing, nextPlacing =
      match player.placing with
        | Some _ -> player.placing, None
        | None ->
          if turns >= 13 then
            assets.CrowdCheerSound.Play() |> ignore // make the crowd cheer to congradulate the player for finishing
            twice(Some(lastPlacing + 1))
          else twice(None)
    // If the player is colliding on the front, then the player is crashing
    match collisionResults with
      | true :: _ ->
        snd.Stop()
        Player.Crashed(new CrashedData(player.bounds, player.placing)), None
      | _ ->
        Player.Moving(
          new MovingData(
            new PlayerShape(position, player.bounds.Width, player.bounds.Height, direction),
            ((player.velocity * 128.0) + input.power) / 129.0, turns, lastTurnedLeft, taunt, tauntTimer, collisionResults, placing)), nextPlacing
  | Crashed _ -> player, None

// ===================
// == XNA DEPENDENT ==
// ===================
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

// Renders a player, assuming spriteBatch.Begin has already been called
let drawPlayer (sb: SpriteBatch, rect: Rectangle) player isMainPlayer (assets: GameContent) fontBatch =
  let playerBounds, playerIL =
    match player with
    | Moving player -> player.bounds, player.intersectingLines
    | Crashed player -> player.bounds, [false; false; false; false]
  sb.Draw(
    assets.ChariotTexture, playerBounds.Center, new Nullable<_>(), Color.White, single playerBounds.Direction,
    (float32 assets.ChariotTexture.Width / 2.0f @@ float32 assets.ChariotTexture.Height / 2.0f),
    1.0f, // scale
    SpriteEffects.None, single 0)
#if DEBUG
  playerBounds.Draw(sb, assets.Pixel, playerIL)
#endif
  match player with
  | Moving player ->
    // Draw the player's taunt, if any
    match player.currentTaunt with
    | Some taunt ->
      FlatSpriteFont.drawString
        assets.Font fontBatch taunt player.position 2.0f
        (if isMainPlayer then Color.White else Color.OrangeRed)
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed player ->
    // Remind the player that they are crashed
    let message, color = if isMainPlayer then "Strepebas!", Color.Red else "Strepebat!", (new Color(Color.Red, 63))
    FlatSpriteFont.drawString
      assets.Font fontBatch message player.position 3.0f color
      (FlatSpriteFont.Center, FlatSpriteFont.Center)