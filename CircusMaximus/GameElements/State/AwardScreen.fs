namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type AwardScreen(playerDataAndWinnings, playerHorses, buttonGroup) =
  /// A list of player data and the amounts they just earned
  member this.playerDataAndWinnings = playerDataAndWinnings
  /// A list of players' horses saved from the last race, which will be passed onto the next
  member this.playerHorses: Horses list = playerHorses
  member this.buttonGroup = buttonGroup
  
  interface IGameScreen with
    member this.Next deltaTime rand input = AwardScreen.next this deltaTime rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  /// The default initialized award screen
  static member init fields playerDataAndWinnings playerHorses =
    let x = fields.settings.windowDimensions.X / 2.f
    let y = fields.settings.windowDimensions.Y / 10.f
    let inline initb i label =
      Button.initCenter
        (x @@ y * i)
        Button.defaultButtonDimensions label
    new AwardScreen(playerDataAndWinnings, playerHorses, ButtonGroup.init [initb 1.f "Contine"; initb 2.f "Exi cursus"]),
    playerDataAndWinnings |> List.map
        (fun (playerData, winnings) -> { playerData with coinBalance = playerData.coinBalance + winnings })