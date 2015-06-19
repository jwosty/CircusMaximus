namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Core.LanguagePrimitives
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

/// Represents the current state a button is in
type ButtonState =
  | Pressing | Down
  | Releasing | Up

type Button =
  { position: Vector2<px>
    dimensions: Vector2<px>
    label: string
    buttonState: ButtonState
    isSelected: bool }
  /// The button's bottom left corner coordinates
  member this.bottomLeft = this.position + this.dimensions
  
  static member defaultButtonDimensions = 512<px> @@ 64<px>
  
  /// Initialize a button at the given position with the given dimensions and label
  static member init position dimensions label =
    { position = position; dimensions = dimensions
      label = label; buttonState = Up
      isSelected = false }
  
  /// Initialize a button at a given center with the given dimensions and label
  static member initCenter (center: Vector2<_>) dimensions label =
    { position = center - (dimensions / 2.)
      dimensions= dimensions; label = label
      buttonState = Up; isSelected = false }