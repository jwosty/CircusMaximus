namespace CircusMaximus.Functions
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

module BoundParticle =
  let nextParticle particle =
    { particle with
        position = positionForward particle.position particle.direction (unitlessCos (particle.age / 64.<fr>) * particle.factor * 1.<px>)
        age = particle.age + 1.<fr> }