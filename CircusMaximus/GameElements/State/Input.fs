namespace CircusMaximus.Input
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

type PlayerInput =
  { /// Forward push; ranges from 0 to 1
    power: float
    /// Turn amount; ranges from 0 to 1
    turn: float
    /// True when the player presses the taunt button (Q on the keyboard, A on the controller)
    expectingTaunt: bool
    /// Debug input to advance one lap
    advanceLap: bool }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerInput =
  let initFromKeyboard (lastKeyboard: KeyboardState, keyboard: KeyboardState) =
    { power = if keyboard.IsKeyDown(Keys.W) then 1. else 0.0
      turn =
        (  if keyboard.IsKeyDown(Keys.A) then -1. else 0.0)
        + (if keyboard.IsKeyDown(Keys.D) then 1. else 0.0)
        |> degreesToRadians
      expectingTaunt = keyboard.IsKeyDown(Keys.Q)
      advanceLap = lastKeyboard.IsKeyDown(Keys.L) && keyboard.IsKeyUp(Keys.L) }
  
  let initFromGamepad (lastGamepad: GamePadState, gamepad: GamePadState) =
    { power = float gamepad.Triggers.Right
      turn = float gamepad.ThumbSticks.Left.X |> degreesToRadians
      expectingTaunt = (gamepad.Buttons.A = ButtonState.Pressed)
      advanceLap = lastGamepad.IsButtonDown(Buttons.Y) && gamepad.IsButtonUp(Buttons.Y) }