namespace CircusMaximus.Input
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

type PlayerInput =
  { /// Forward push
    power: float
    /// Turn amount
    turn: float
    /// Debug input to advance one lap
    advanceLap: bool }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerInput =
  let maxTurn, maxSpeed = 1.0, 4.0

  let initFromKeyboard (lastKeyboard: KeyboardState, keyboard: KeyboardState) maxTurn maxSpeed =
    { power = if keyboard.IsKeyDown(Keys.W) then maxSpeed else 0.0
      turn =
        (  if keyboard.IsKeyDown(Keys.A) then -maxTurn else 0.0)
        + (if keyboard.IsKeyDown(Keys.D) then maxTurn else 0.0)
        |> degreesToRadians;
      advanceLap = lastKeyboard.IsKeyDown(Keys.L) && keyboard.IsKeyUp(Keys.L) }
  
  let initFromGamepad (lastGamepad: GamePadState, gamepad: GamePadState) maxTurn maxSpeed =
    { power = float gamepad.Triggers.Right * maxSpeed;
      turn = float gamepad.ThumbSticks.Left.X * maxTurn |> degreesToRadians;
      advanceLap = lastGamepad.IsButtonDown(Buttons.Y) && gamepad.IsButtonUp(Buttons.Y) }