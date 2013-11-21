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
    /// Debug input to advance one lap
    val advanceLap: bool
    
    new(lastKeyboard: KeyboardState, keyboard: KeyboardState, maxTurn, maxSpeed) =
      { power = if keyboard.IsKeyDown(Keys.W) then maxSpeed else 0.0;
        turn =
          (  if keyboard.IsKeyDown(Keys.A) then -maxTurn else 0.0)
          + (if keyboard.IsKeyDown(Keys.D) then maxTurn else 0.0)
          |> degreesToRadians;
        advanceLap = lastKeyboard.IsKeyDown(Keys.L) && keyboard.IsKeyUp(Keys.L) }
    new(lastKeyboard: KeyboardState, keyboard) = PlayerInputState(keyboard, lastKeyboard, maxTurn, maxSpeed)
    
    new(lastGamepad: GamePadState, gamepad: GamePadState, maxTurn, maxSpeed) =
      { power = float gamepad.Triggers.Right * maxSpeed;
        turn = float gamepad.ThumbSticks.Left.X * maxTurn |> degreesToRadians;
        advanceLap = lastGamepad.IsButtonDown(Buttons.Y) && gamepad.IsButtonUp(Buttons.Y) }
    new(lastGamepad: GamePadState, gamepad) = PlayerInputState(gamepad, lastGamepad, maxTurn, maxSpeed)
    
    new(power, turn, advanceLap) = { power = power; turn = turn; advanceLap = advanceLap }
  end