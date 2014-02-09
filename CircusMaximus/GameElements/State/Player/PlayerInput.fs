namespace CircusMaximus.Input
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.State
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
    advanceLap: bool
    /// Indicates how many items to move the item selector (negative = left, positive = right, 0 = none)
    selectorΔ: int
    /// Whether or not the user pressed the use item button
    isUsingItem: bool }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerInput =
  let initFromKeyboard (lastKeyboard, keyboard) (settings: GameSettings) =
    let keyJustReleased = keyJustReleased (lastKeyboard, keyboard)
    let keyJustPressed = keyJustPressed (lastKeyboard, keyboard)
    { power = if keyboard.IsKeyDown(Keys.W) then 1. else 0.0
      turn =
        (  if keyboard.IsKeyDown(Keys.A) then -1. else 0.0)
        + (if keyboard.IsKeyDown(Keys.D) then 1. else 0.0)
        |> degreesToRadians
      expectingTaunt = keyboard.IsKeyDown(Keys.Q)
      advanceLap = settings.debugLapIncrement && keyJustPressed Keys.L
      selectorΔ =
        match keyJustPressed Keys.Left, keyJustPressed Keys.Right with
        | true, false -> -1
        | false, true -> 1
        | _ -> 0
      isUsingItem = keyJustPressed Keys.E }
  
  let initFromGamepad (lastGamepad, gamepad) (settings: GameSettings) =
    let gpButtonJustReleased = gpButtonJustReleased (lastGamepad, gamepad)
    let gpButtonJustPressed = gpButtonJustPressed (lastGamepad, gamepad)
    { power = float gamepad.Triggers.Right
      turn = float gamepad.ThumbSticks.Left.X |> degreesToRadians
      expectingTaunt = gamepad.IsButtonDown(Buttons.Y)
      advanceLap = settings.debugLapIncrement && gpButtonJustPressed Buttons.X
      selectorΔ =
        match gpButtonJustPressed Buttons.LeftShoulder, gpButtonJustPressed Buttons.RightShoulder with
        | true, false -> -1
        | false, true -> 1
        | _ -> 0
      isUsingItem = gpButtonJustPressed Buttons.A}