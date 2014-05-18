namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

/// Represents a player's collective horses
type Horses =
  { /// Acceleration, from 0 to 1
    acceleration: float
    /// Top speed, from 0 to 1
    topSpeed: float
    /// Turn speed, from 0 to 1
    turn: float }

type HorseScreen private(next, horses: Horses list, buttons: ButtonGroup) =
  member this.horses = horses
  member this.buttons = buttons
  
  interface IGameScreen with
    member this.Next rand input = next this rand input
  
  static member init horsesNext horses buttons =
    new HorseScreen(horsesNext, horses, buttons)