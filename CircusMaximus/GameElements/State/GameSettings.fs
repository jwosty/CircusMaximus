namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

type GameSettings =
  { windowDimensions: Vector2<px>
    debugDrawBounds: bool
    debugLapIncrement: bool }