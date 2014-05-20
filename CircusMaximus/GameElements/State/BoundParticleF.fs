namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

module BoundParticle =
  let nextParticle particle =
    { particle with
        position = positionForward particle.position particle.direction (cos(particle.age / 64.) * particle.factor)
        age = particle.age + 1. }