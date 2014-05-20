namespace CircusMaximus.Functions
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
  let next input (button: Button) =
    let leftDown = input.mouse.LeftButton = Input.ButtonState.Pressed
    let leftUp = not leftDown
    let mouseInBounds =
      (   float32 input.mouse.Position.X > button.position.X    && float32 input.mouse.Position.Y > button.position.Y)
      && (float32 input.mouse.Position.X < button.bottomLeft.X  && float32 input.mouse.Position.Y < button.bottomLeft.Y)
    let isInputPressing =
      (button.isSelected && ((input.keyboard.IsKeyDown Keys.Space) || input.keyboard.IsKeyDown Keys.Enter)) ||
      (button.isSelected && (List.exists (fun (gamepad: GamePadState) -> gamepad.IsButtonDown Buttons.A) input.gamepads)) ||
      (leftDown && mouseInBounds)
    { button with
        buttonState =
          match button.buttonState with
          | Releasing | Up -> if isInputPressing then Pressing else Up
          | Pressing | Down -> if not isInputPressing then Releasing else Down }