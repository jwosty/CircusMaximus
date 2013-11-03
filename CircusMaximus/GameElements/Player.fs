module CircusMaximus.Player
open System

// =====================
// == XNA INDEPENDENT ==
// =====================
open Microsoft.Xna.Framework
open Extensions
open HelperFunctions
open BoundingBox2D
open LineSegment

let tauntTime = 500

type Player =
  struct
    /// The bounding box that stores the player's position, dimensions, and directions
    val public boundingBox: BoundingBox2D.BoundingBox2D
    val public velocity: float
    val public turns: int
    val public lastTurnedLeft: bool
    val public currentTaunt: string option
    val public tauntTimer: int
    val public intersectingLines: bool list
    
    new(bb, vel, turns, ltl, tnt, tntT, il) =
      { boundingBox = bb; velocity = vel; turns = turns;
      lastTurnedLeft = ltl; currentTaunt = tnt; tauntTimer = tntT;
      intersectingLines = il }
    
    new(bb, vel, center: Vector2) =
      { boundingBox = bb; velocity = vel;
        turns = if bb.Center.Y >= center.Y then 0 else -1;
        // Always start on the opposite side
        lastTurnedLeft = bb.Center.X >= center.X;
        currentTaunt = None; tauntTimer = 0;
        intersectingLines = [] }
    
    /// Player position, obtained from the bounding box
    member this.position with get() = this.boundingBox.Center
    /// Player direction, in radians, obtained from the bounding box
    member this.direction with get() = this.boundingBox.Direction
  end

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

/// Tests for a collision playerA and all other players
let findCollisions (player: Player) (otherPlayers: Player list) =
  otherPlayers
    |> List.map (fun otherPlayer -> player.boundingBox.FindIntersections otherPlayer.boundingBox)
    |> List.combine (||)

#nowarn "49"
/// Returns the next position and direction of the player and change in direction
let nextPositionDirection otherPlayers (player: Player) Δdirection =
  (player.position
    + (   cos player.direction * player.velocity
       @@ sin player.direction * player.velocity),
   player.direction + Δdirection)

/// Returns the next number of laps and whether or not the player last turned on the left side of the map
let updateLaps racetrackCenter (player: Player) nextPosition =
  if isPassingTurnLine racetrackCenter player.lastTurnedLeft player.position nextPosition then
    player.turns + 1, not player.lastTurnedLeft
  else
    player.turns, player.lastTurnedLeft

/// Returns a new taunt if needed, otherwise none
let updateTaunt (player: Player) expectingTaunt =
  if player.tauntTimer > 0 then
    player.currentTaunt, player.tauntTimer - 1
  elif expectingTaunt then
    Some(Taunt.pickTaunt ()), tauntTime
  else
    None, player.tauntTimer - 1

/// Update the player with the given parameters, but this is functional, so it won't actually modify anything
let update (Δdirection, nextVelocity) otherPlayers (player: Player) expectingTaunt (racetrackCenter: Vector2) =
  let position, direction = nextPositionDirection otherPlayers player Δdirection
  // If the player has crossed the threshhold not more than once in a row, increment the turn count
  let turns, lastTurnedLeft = updateLaps racetrackCenter player position
  let taunt, tauntTimer = updateTaunt player expectingTaunt
  
  new Player(
    new BoundingBox2D(position, direction, player.boundingBox.Width, player.boundingBox.Height),
    nextVelocity, turns, lastTurnedLeft, taunt, tauntTimer, findCollisions player otherPlayers)


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
  sb.Draw(
    texture, player.position, new Nullable<_>(), Color.White, single player.direction,
    (float32 texture.Width / 1.75f @@ float32 texture.Height / 1.75f),
    1.0f, // scale
    SpriteEffects.None, single 0)
#if DEBUG
  player.boundingBox.Draw(sb, pixelTexture, player.intersectingLines)
#endif
  // Draw the player's taunt, if any
  match player.currentTaunt with
    | Some taunt ->
        FlatSpriteFont.drawString
          font fontBatch taunt player.position 2.0f
          (if isMainPlayer then Color.White else Color.OrangeRed)
          (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()