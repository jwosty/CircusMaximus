namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types.UnitSymbols

/// Represents a player's collective horses
type Horses =
  { /// Acceleration, from 0 to 1
    acceleration: float<px/fr^2>
    /// Top speed, from 0 to 1
    topSpeed: float<px/fr>
    /// Turn speed, from 0 to 1
    turn: float }