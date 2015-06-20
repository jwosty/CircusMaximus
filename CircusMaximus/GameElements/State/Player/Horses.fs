namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types.UnitSymbols

/// Represents a player's collective horses
type Horses =
  { /// Acceleration, from 0 to 1
    acceleration: float<px/s^2>
    /// Top speed, from 0 to 1
    topSpeed: float<px/s>
    /// Turn speed, from 0 to 1
    turn: float }