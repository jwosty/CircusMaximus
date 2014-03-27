module CircusMaximus.Types.Racetrack
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision

let center = 5418 @@ 1255

let collisionShape = new RacetrackSpinaShape(center)
let collisionBounds = BoundingPolygon(collisionShape)