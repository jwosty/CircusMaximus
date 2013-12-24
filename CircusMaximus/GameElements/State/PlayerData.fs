namespace CircusMaximus.State
open System
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// A record for holding long-term information about a player (e.g. money)
type PlayerData =
  { index: int
    // TODO: base values on real Roman currency
    coinBalance: int }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerData =
  /// Initializes a PlayerData with default values
  let initEmpty index =
    { index = index
      coinBalance = 0 }