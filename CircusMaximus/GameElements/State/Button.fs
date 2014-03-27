namespace CircusMaximus.Types
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