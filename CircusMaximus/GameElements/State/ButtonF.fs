﻿namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

module Button =
  let basicNext (button: Button) isSelected =
    { button with
        buttonState =
          match button.buttonState with
          | Releasing | Up ->   if isSelected then Pressing else Up
          | Pressing  | Down -> if not isSelected then Releasing else Down }
  
  /// Returns the next button state, taking into account the input devices
  let next (mouse: MouseState) (keyboard: KeyboardState) gamepads (button: Button) =
    let leftDown = mouse.LeftButton = Input.ButtonState.Pressed
    let leftUp = not leftDown
    let mouseInBounds =
      (   float32 mouse.Position.X > button.position.X    && float32 mouse.Position.Y > button.position.Y)
      && (float32 mouse.Position.X < button.bottomLeft.X  && float32 mouse.Position.Y < button.bottomLeft.Y)
    let isInputPressing =
      (button.isSelected && ((keyboard.IsKeyDown Keys.Space) || keyboard.IsKeyDown Keys.Enter)) ||
      (button.isSelected && (List.exists (fun (gamepad: GamePadState) -> gamepad.IsButtonDown Buttons.A) gamepads)) ||
      (leftDown && mouseInBounds)
    { button with
        buttonState =
          match button.buttonState with
          | Releasing | Up -> if isInputPressing then Pressing else Up
          | Pressing | Down -> if not isInputPressing then Releasing else Down }