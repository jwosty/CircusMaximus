namespace CircusMaximus.Types
open System
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

/// A record for holding long-term information about a player (e.g. money)
type PlayerData =
  { number: int
    // TODO: base values on real Roman currency
    coinBalance: int }