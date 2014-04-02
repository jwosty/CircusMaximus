namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type ButtonGroup =
  { buttons: Button list
    selected: int }
  
  /// Initializes a button group with the given buttons
  static member init buttons =
    { buttons = buttons
      selected = 0 }