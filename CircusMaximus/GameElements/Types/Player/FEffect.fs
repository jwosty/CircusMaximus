namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Effect =
  /// Updates a list of effects, removing old ones and aging the rest
  let nextEffects (effects: Effect list) =
    effects
      |> List.map (fun (e, d) -> e, d - 1)
      |> List.filter (fun (_, d) -> d > 0)
  
  /// Finds the effect that matches the given effect, using the one with the greatest remaining duration
  let findLongest (effects: Effect list) key =
    let effects = List.filter (fun (e, _) -> e = key) effects
    match effects with
    | [] -> None
    | _ -> Some(List.maxBy snd effects)