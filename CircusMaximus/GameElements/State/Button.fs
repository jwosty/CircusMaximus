namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// Represents the current state a button is in
type ButtonState =
  | Pressing | Down
  | Releasing | Up

type Button =
  { position: Vector2
    width: int; height: int
    label: string
    buttonState: ButtonState
    isSelected: bool }
  /// The button's bottom left corner coordinates
  member this.bottomLeft = (this.position.X + float32 this.width) @@ (this.position.Y + float32 this.height)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Button =
  let defaultButtonSize = 512, 64
  
  let init position (width, height) label =
    { position = position
      width = width; height = height
      label = label
      buttonState = Up
      isSelected = false }
  
  let initCenter (center: Vector2) (width, height) label =
    { position = (center.X - (float32 width / 2.0f) @@ center.Y - (float32 height / 2.0f))
      width = width; height = height
      label = label
      buttonState = Up
      isSelected = false }
  
  let basicNext (button: Button) isSelected =
    { button with
        buttonState =
          match button.buttonState with
          | Releasing | Up ->   if isSelected then Pressing else Up
          | Pressing  | Down -> if not isSelected then Releasing else Down }
  
  /// Returns the next button state, taking into account the input devices
  let next (mouse: MouseState) gamepads (button: Button) =
    let leftDown = mouse.LeftButton = Input.ButtonState.Pressed
    let leftUp = not leftDown
    let mouseInBounds =
      (   float32 mouse.Position.X > button.position.X    && float32 mouse.Position.Y > button.position.Y)
      && (float32 mouse.Position.X < button.bottomLeft.X  && float32 mouse.Position.Y < button.bottomLeft.Y)
    let isInputPressing =
      (leftDown && mouseInBounds) ||
      (button.isSelected && (List.exists (fun (gamepad: GamePadState) -> gamepad.IsButtonDown Buttons.A) gamepads))
    { button with
        buttonState =
          match button.buttonState with
          | Releasing | Up -> if isInputPressing then Pressing else Up
          | Pressing | Down -> if not isInputPressing then Releasing else Down }