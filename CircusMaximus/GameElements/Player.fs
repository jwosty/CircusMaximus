module CircusMaximus.Player
open System

// =====================
// == XNA INDEPENDENT ==
// =====================
open Microsoft.Xna.Framework
open Extensions
open HelperFunctions
open PlayerInput

let tauntTime = 1000

type Player =
  | Moving of State.Player.Moving
  | Crashed of State.Player.Crashed

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

let playerBB = function
  | Moving player -> player.collisionBox
  | Crashed player -> player.collisionBox

let playerPlacing = function
  | Moving player -> player.placing
  | Crashed player -> player.placing

#nowarn "49"
/// Returns the next position and direction of the player and change in direction
let nextPositionDirection (player: State.Player.Moving) Δdirection =
  (player.position
    + (   cos player.direction * player.velocity
       @@ sin player.direction * player.velocity),
   player.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let updateLaps racetrackCenter (input: PlayerInputState) (player: State.Player.Moving) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition || input.advanceLap then
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
let update (input: PlayerInputState) (player: Player) collisionResults lastPlacing expectingTaunt (racetrackCenter: Vector2) =
  match player with
  | Moving player ->
    //let movingPlayers = filterMoving otherPlayers
    let position, direction = nextPositionDirection player input.turn
    // If the player has crossed the threshhold not more than once in a row, increment the turn count
    let turns, lastTurnedLeft = updateLaps racetrackCenter input player position
    let taunt, tauntTimer = updateTaunt player expectingTaunt
    let placing, nextPlacing =
      match player.placing with
        | Some _ -> player.placing, None
        // 3 turns only for the presentation of this project
        | None -> if turns >= 13 then twice(Some(lastPlacing + 1)) else twice(None)
    
    //let collisions = detectCollisions player otherPlayers
    // If the player is colliding on the front, then the player is crashing
    match collisionResults with
      | true :: _ -> Player.Crashed(new State.Player.Crashed(player.boundingBox, player.placing)), None
      | _ ->
        Player.Moving(
          new State.Player.Moving(
            new OrientedRectangle(position, player.boundingBox.Width, player.boundingBox.Height, direction),
            ((player.velocity * 128.0) + input.power) / 129.0, turns, lastTurnedLeft, taunt, tauntTimer, collisionResults, placing)), nextPlacing
  | Crashed _ -> player, None

// ===================
// == XNA DEPENDENT ==
// ===================

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Input

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
    let message, color = if isMainPlayer then "Strepebas!", Color.Red else "Strepebant!", (new Color(Color.Red, 63))
    FlatSpriteFont.drawString
      font fontBatch message player.position 3.0f color
      (FlatSpriteFont.Center, FlatSpriteFont.Center)