namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type Ability =
  /// In addition to the normal slow of turn velocity, taunting additionally slows velocity
  | TauntSlow