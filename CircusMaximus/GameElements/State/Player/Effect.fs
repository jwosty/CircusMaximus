namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type EffectType = | Taunted | Sugar
type Effect = EffectType * Duration

module EffectDurations =
  let taunt = 750
  let sugar = 60