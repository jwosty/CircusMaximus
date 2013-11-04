module CircusMaximus.Player
open System

// =====================
// == XNA INDEPENDENT ==
// =====================
open Microsoft.Xna.Framework
open Extensions
open HelperFunctions

let tauntTime = 500

type Player =
  | Moving of State.Player.Moving
  | Crashed of State.Player.Crashed

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

/// Tests for a collision playerA and all other players
let detectCollisions (player: State.Player.Moving) (otherPlayers: Player list) =
  otherPlayers
    |> List.map
         (fun otherPlayer ->
            let otherPlayerBB = match otherPlayer with | Moving player -> player.boundingBox | Crashed player -> player.boundingBox
            Collision.collide_ORect_ORect player.boundingBox otherPlayerBB)//player.boundingBox.FindIntersections otherPlayerBB)
    |> List.combine (||)

#nowarn "49"
/// Returns the next position and direction of the player and change in direction
let nextPositionDirection (player: State.Player.Moving) Δdirection =
  (player.position
    + (   cos player.direction * player.velocity
       @@ sin player.direction * player.velocity),
   player.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let updateLaps racetrackCenter (player: State.Player.Moving) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition then
    player.turns + 1, not player.lastTurnedLeft
  else
    player.turns, player.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let updateTaunt (player: State.Player.Moving) expectingTaunt =
  if player.tauntTimer > 0 then
    player.currentTaunt, player.tauntTimer - 1
  elif expectingTaunt then
    Some(Taunt.pickTaunt ()), tauntTime
  else
    None, player.tauntTimer - 1

/// Update the player with the given parameters, but this is functional, so it won't actually modify anything
let update (Δdirection, nextVelocity) otherPlayers (player: Player) expectingTaunt (racetrackCenter: Vector2) =
  match player with
  | Moving player ->
    //let movingPlayers = filterMoving otherPlayers
    let position, direction = nextPositionDirection player Δdirection
    // If the player has crossed the threshhold not more than once in a row, increment the turn count
    let turns, lastTurnedLeft = updateLaps racetrackCenter player position
    let taunt, tauntTimer = updateTaunt player expectingTaunt
    
    let collisions = detectCollisions player otherPlayers
    // If the player is colliding on the front, then the player is crashing
    if List.head collisions then
      Player.Crashed(new State.Player.Crashed(player.boundingBox))
    else
      Player.Moving(
        new State.Player.Moving(
          new OrientedRectangle(position, direction, player.boundingBox.Width, player.boundingBox.Height),
          nextVelocity, turns, lastTurnedLeft, taunt, tauntTimer, collisions))
  | Crashed _ -> player

// ===================
// == XNA DEPENDENT ==
// ===================

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Input

let degreesToRadians d = 2.0 * Math.PI / 360.0 * d

let private maxTurn, maxSpeed = 1.0, 4.0

// Returns change in direction and power (in that order) based on the given game pad state
let getPowerTurnFromGamepad(gamepad: GamePadState) =
  (float gamepad.ThumbSticks.Left.X * maxTurn |> degreesToRadians, float gamepad.Triggers.Right * maxSpeed)

// Returns change in direction and power (in that order) based on the given keyboard state
let getPowerTurnFromKeyboard(keyboard: KeyboardState) =
  ( (if keyboard.IsKeyDown(Keys.A) then -maxTurn else 0.0) + (if keyboard.IsKeyDown(Keys.D) then maxTurn else 0.0)
      |> degreesToRadians,
    (if keyboard.IsKeyDown(Keys.W) then maxSpeed else 0.0))

let loadContent (content: ContentManager) =
  content.Load<Texture2D>("chariot")

// Renders a player, assuming spriteBatch.Begin has already been called
let draw (sb: SpriteBatch, rect: Rectangle) (player: Player) isMainPlayer (texture: Texture2D) font fontBatch pixelTexture =
  let playerBB, playerIL =
    match player with
    | Moving player -> player.boundingBox, player.intersectingLines
    | Crashed player -> player.boundingBox, [false; false; false; false]
  sb.Draw(
    texture, playerBB.Center, new Nullable<_>(), Color.White, single playerBB.Direction,
    (float32 texture.Width / 2.0f @@ float32 texture.Height / 2.0f),
    1.0f, // scale
    SpriteEffects.None, single 0)
#if DEBUG
  playerBB.Draw(sb, pixelTexture, playerIL)
#endif
  match player with
  | Moving player ->
    // Draw the player's taunt, if any
    match player.currentTaunt with
    | Some taunt ->
      FlatSpriteFont.drawString
        font fontBatch taunt player.position 2.0f
        (if isMainPlayer then Color.White else Color.OrangeRed)
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed player ->
    // Remind the player that they are crashed
    FlatSpriteFont.drawString
      font fontBatch "CRASHED" player.position 3.0f
      (if isMainPlayer then Color.Red else (new Color(Color.Red, 63)))
      (FlatSpriteFont.Center, FlatSpriteFont.Center)