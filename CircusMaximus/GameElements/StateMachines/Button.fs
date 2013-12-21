namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type ButtonState =
  | Pressing | Down
  | Releasing | Up

type Button =
  { position: Vector2; width: int; height: int; buttonState: ButtonState }
  /// The button's bottom left corner coordinates
  member this.bottomLeft = (this.position.X + float32 this.width) @@ (this.position.Y + float32 this.height)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Button =
  let init position (width, height) = { position = position; buttonState = Up; width = width; height = height }
  
  let initCenter (center: Vector2) (width, height) =
    { position = (center.X - float32 width @@ center.Y - float32 height)
      width = width; height = height
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