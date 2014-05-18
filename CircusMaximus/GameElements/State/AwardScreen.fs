namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type AwardScreen(timer, playerDataAndWinnings, playerHorses, buttonGroup) =
  member this.timer = timer
  /// A list of player data and the amounts they just earned
  member this.playerDataAndWinnings = playerDataAndWinnings
  /// A list of players' horses saved from the last race, which will be passed onto the next
  member this.playerHorses: Horses list = playerHorses
  member this.buttonGroup = buttonGroup
  
  interface IGameScreen with
    member this.Next rand input = AwardScreen.next this rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  /// The default initialized award screen
  static member init fields playerDataAndWinnings playerHorses =
    let x = fields.settings.windowDimensions.X / 2.f
    let y = fields.settings.windowDimensions.Y / 10.f
    let inline initb i label =
      Button.initCenter
        (x @@ y * i)
        Button.defaultButtonSize label
    new AwardScreen(0, playerDataAndWinnings, playerHorses, ButtonGroup.init [initb 1.f "Contine"; initb 2.f "Exi cursus"]),
    playerDataAndWinnings |> List.map
        (fun (playerData, winnings) -> { playerData with coinBalance = playerData.coinBalance + winnings })