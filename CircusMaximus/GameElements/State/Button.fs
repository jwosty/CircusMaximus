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
  
  static member defaultButtonSize = 512, 64
  
  /// Initialize a button at the given position with the given dimensions and label
  static member init position (width, height) label =
    { position = position
      width = width; height = height
      label = label
      buttonState = Up
      isSelected = false }
  
  /// Initialize a button at a given center with the given dimensions and label
  static member initCenter (center: Vector2) (width, height) label =
    { position = (center.X - (float32 width / 2.0f) @@ center.Y - (float32 height / 2.0f))
      width = width; height = height
      label = label
      buttonState = Up
      isSelected = false }