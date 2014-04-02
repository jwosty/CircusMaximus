namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type AwardScreen =
  { timer: int
    /// A list of player data and the amounts they just earned
    playerDataAndWinnings: (PlayerData * int) list
    /// A list of players' horses saved from the last race, which will be passed onto the next
    playerHorses: Horses list
    buttonGroup: ButtonGroup }
  
  /// The default initialized award screen
  static member init (settings: GameSettings) (playerDataAndWinnings: (PlayerData * int) list) playerHorses =
    let x = settings.windowDimensions.X / 2.f
    let y8 = settings.windowDimensions.Y / 10.f
    let inline initb i label =
      Button.initCenter
        (x @@ y8 * i)
        Button.defaultButtonSize label
    // The new playerData that the game should now use
    let newPlayerData =
      playerDataAndWinnings |> List.map
        (fun (playerData, winnings) -> { playerData with coinBalance = playerData.coinBalance + winnings })
    { timer = 0
      playerDataAndWinnings = playerDataAndWinnings
      playerHorses = playerHorses
      buttonGroup = ButtonGroup.init [initb 1.f "Contine"; initb 2.f "Exi cursus"] },
    newPlayerData