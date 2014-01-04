namespace CircusMaximus.State
open System
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// A record for holding long-term information about a player (e.g. money)
type PlayerData =
  { number: int
    // TODO: base values on real Roman currency
    coinBalance: int }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerData =
  /// Initializes a PlayerData with default values
  let initEmpty number =
    { number = number
      coinBalance = 0 }
  
  let playerWinnings placing =
    match placing with
    | 1 -> 100
    | 2 -> 50
    | 3 -> 25
    | _ -> 0