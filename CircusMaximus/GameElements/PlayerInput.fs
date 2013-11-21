module CircusMaximus.PlayerInput
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

let maxTurn, maxSpeed = 1.0, 4.0

type PlayerInputState =
  struct
    /// Forward push
    val power: float
    /// Turn amount
    val turn: float
    
    new(keyboard: KeyboardState, maxTurn, maxSpeed) =
      { power = if keyboard.IsKeyDown(Keys.W) then maxSpeed else 0.0;
        turn =
          (  if keyboard.IsKeyDown(Keys.A) then -maxTurn else 0.0)
          + (if keyboard.IsKeyDown(Keys.D) then maxTurn else 0.0)
          |> degreesToRadians }
    new(keyboard: KeyboardState) = PlayerInputState(keyboard, maxTurn, maxSpeed)
    
    new(gamepad: GamePadState, maxTurn, maxSpeed) =
      { power = float gamepad.Triggers.Right * maxSpeed;
        turn = float gamepad.ThumbSticks.Left.X * maxTurn |> degreesToRadians }
    new(gamepad: GamePadState) = PlayerInputState(gamepad, maxTurn, maxSpeed)
    
    new(power, turn) = { power = power; turn = turn }
  end