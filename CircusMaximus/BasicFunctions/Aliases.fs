namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// A velocity, in percentage (where 0 = 0% and 1 = 100%) of a top speed
type Velocity = float
/// A player's finishing place in the race
type Placing = int
/// A length of time
type Duration = int
/// A taunt contains the string to display as well as the amount of time it lasts for
type Taunt = string * Duration