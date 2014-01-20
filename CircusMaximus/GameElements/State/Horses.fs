namespace CircusMaximus.State
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