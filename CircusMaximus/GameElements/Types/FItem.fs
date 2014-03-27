namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Item =
  /// Gets and item's appropriate effect when used
  let getEffect item : Effect =
    match item with
    | Item.SugarCubes -> EffectType.Sugar, EffectDurations.sugar