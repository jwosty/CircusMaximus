module CircusMaximus.Types.Racetrack
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Collision
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

let center = 5418<px> @@ 1255<px>

let collisionShape = new RacetrackSpinaShape(center)
let collisionBounds = BoundingPolygon(collisionShape)