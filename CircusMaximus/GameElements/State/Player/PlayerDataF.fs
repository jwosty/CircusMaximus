namespace CircusMaximus.Functions
open System
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PlayerData =
  let playerWinnings placing =
    match placing with
    | 1 -> 100
    | 2 -> 50
    | 3 -> 25
    | _ -> 0
  
  let findByNumber number playerData = List.find (fun (playerData: PlayerData) -> playerData.number = number) playerData