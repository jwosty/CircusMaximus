namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions

type ButtonState = | Pressed | Released

type Button = { buttonState: ButtonState; position: Vector2 }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Button =
  let init position = { buttonState = Released; position = position }
  
  let initCenter (center: Vector2) (width, height) =
    { buttonState = Released
      position = (center.X - width @@ center.Y - height) }
  
  /// Returns the next button state with the given conditions
  let next button lastInput input = button