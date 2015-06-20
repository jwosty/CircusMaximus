namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

type GameSettings =
  { windowDimensions: Vector2<px>
    debugDrawAccelerationTimer: bool
    debugDrawBounds: bool
    debugLapIncrement: bool }