namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// Represents the current state a button is in. Click actions should be taken if the button state
/// is Releasing.
type ButtonState =
  | Pressing | Down
  | Releasing | Up

type Button =
  { position: Vector2
    width: int; height: int
    label: string
    buttonState: ButtonState }
  /// The button's bottom left corner coordinates
  member this.bottomLeft = (this.position.X + float32 this.width) @@ (this.position.Y + float32 this.height)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Button =
  let defaultButtonSize = 512, 64
  
  let init position (width, height) label =
    { position = position
      width = width; height = height
      label = label
      buttonState = Up }
  
  let initCenter (center: Vector2) (width, height) label =
    { position = (center.X - (float32 width / 2.0f) @@ center.Y - (float32 height / 2.0f))
      width = width; height = height
      label = label
      buttonState = Up }
  
  /// Returns the next button state with the given conditions
  let next (button: Button) (mouse: MouseState) =
    let leftDown = mouse.LeftButton = Input.ButtonState.Pressed
    let leftUp = not leftDown
    let mouseInBounds =
      (   float32 mouse.Position.X > button.position.X    && float32 mouse.Position.Y > button.position.Y)
      && (float32 mouse.Position.X < button.bottomLeft.X  && float32 mouse.Position.Y < button.bottomLeft.Y)
    let nextState =
      match button.buttonState with
      | Releasing ->  if leftDown && mouseInBounds  then Pressing   else Up
      | Up ->         if leftDown && mouseInBounds  then Pressing   else Up
      | Pressing ->   if leftUp                     then Releasing  else Down
      | Down ->       if leftUp                     then Releasing  else Down
    { button with buttonState = nextState }