namespace CircusMaximus.Input
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.State
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

type PlayerInput =
  { /// How hard the player is pulling on the left reign; ranges from 0 to 1
    leftReignPull: float
    /// How hard the player is puling on the right reign; ranges from 0 to 1
    rightReignPull: float
    /// Whether or not the player is pressing the button to flick the reigns (controls speed)
    flickReigns: bool
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
    { /// TODO: change reign input to be more precise (e.g. Q is pulls left reign lightly, A is harder, and Z is hardest)
      leftReignPull = if keyboard.IsKeyDown Keys.A then 1. else 0.
      /// TODO: change reign input to be more precise (e.g. E is pulls right reign lightly, D is harder, and C is hardest)
      rightReignPull = if keyboard.IsKeyDown Keys.D then 1. else 0.
      flickReigns = keyJustPressed Keys.Space
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
    { leftReignPull = float gamepad.Triggers.Left
      rightReignPull = float gamepad.Triggers.Right
      flickReigns = gpButtonJustPressed Buttons.LeftStick || gpButtonJustPressed Buttons.RightStick
      expectingTaunt = gamepad.IsButtonDown(Buttons.Y)
      advanceLap = settings.debugLapIncrement && gpButtonJustPressed Buttons.X
      selectorΔ =
        match gpButtonJustPressed Buttons.LeftShoulder, gpButtonJustPressed Buttons.RightShoulder with
        | true, false -> -1
        | false, true -> 1
        | _ -> 0
      isUsingItem = gpButtonJustPressed Buttons.A}