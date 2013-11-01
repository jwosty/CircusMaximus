module CircusMaximus.Player
open System

// =====================
// == XNA INDEPENDENT ==
// =====================
open Microsoft.Xna.Framework

let tauntTime = 500

type Player =
  struct
    // This is from XNA, but it's a common mathematical structure so it's OK
    val public position: Microsoft.Xna.Framework.Vector2
    // Radians
    val public direction: float
    val public velocity: float
    val public turns: int
    val public lastTurnedLeft: bool
    val public currentTaunt: string option
    val public tauntTimer: int
    new(pos, dir, vel, turns, ltl, tnt, tntT) = { position = pos; direction = dir; velocity = vel; turns = turns; lastTurnedLeft = ltl; currentTaunt = tnt; tauntTimer = tntT }
    new(pos, dir, vel, (center: Vector2)) =
      { position = pos; direction = dir; velocity = vel;
        turns = if pos.Y >= center.Y then 0 else -1;
        // Always start on the opposite side
        lastTurnedLeft = pos.X >= center.X;
        currentTaunt = None; tauntTimer = 0 }
  end

let isPassingTurnLine (center: Vector2) lastTurnedLeft (lastPosition: Vector2) (position: Vector2) =
  if lastTurnedLeft && position.X > center.X then
    lastPosition.Y > center.Y && position.Y < center.Y
  elif (not lastTurnedLeft) && position.X < center.X then
    lastPosition.Y < center.Y && position.Y > center.Y
  else false

#nowarn "49"
// Returns a new player updated with the given parameters
let update (Δdirection, velocity) (player: Player) expectingTaunt (center: Vector2) =
  let position =
    player.position 
      + new Vector2(
        float32 <| cos player.direction * player.velocity,
        float32 <| sin player.direction * player.velocity)
  let taunt, tauntTimer =
    if player.tauntTimer > 0 then
      player.currentTaunt, player.tauntTimer - 1
    elif expectingTaunt then
      Some(Taunt.pickTaunt ()), tauntTime
    else
      None, player.tauntTimer - 1
  // If the player has crossed the threshhold not more than once in a row, increment the turn count
  let turns, lastTurnedLeft =
    if isPassingTurnLine center player.lastTurnedLeft player.position position then
      player.turns + 1, not player.lastTurnedLeft
    else
      player.turns, player.lastTurnedLeft
  
  new Player(
    position, player.direction + Δdirection, velocity,
    turns, lastTurnedLeft, taunt, tauntTimer)


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
let draw (sb: SpriteBatch, rect: Rectangle) (player: Player) isMainPlayer (texture: Texture2D) font fontBatch =
  // Draw the player
  sb.Draw(
    texture, player.position, new Nullable<_>(), Color.White, single player.direction,
    new Vector2(float32 texture.Width / 1.75f, float32 texture.Height / 1.75f), // texture center
    1.0f, // scale
    SpriteEffects.None, single 0)
  // Draw the player's taunt, if any
  match player.currentTaunt with
    | Some taunt ->
        FlatSpriteFont.drawString
          font fontBatch taunt player.position 2.0f
          (if isMainPlayer then Color.White else Color.OrangeRed)
          (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()