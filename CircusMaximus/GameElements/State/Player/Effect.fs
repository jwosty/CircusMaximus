namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type EffectType = | VelocityDecreased | VelocityIncreased | TurnVelocityDecreased
type Effect = EffectType * Duration

module EffectDurations =
  [<Literal>]
  let taunt = 750
  [<Literal>]
  let sugar = 60