namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AwardScreen =
  /// The default initialized award screen
  let init (settings: GameSettings) (playerDataAndWinnings: (PlayerData * int) list) playerHorses =
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
  
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) (lastKeyboard, keyboard) mouse gamepads =
    let inline buttonState label = ButtonGroup.buttonState awardScreen.buttonGroup label
    match buttonState "Contine", buttonState "Exi cursus" with
    | Releasing, _ -> SwitchToRaces(awardScreen.playerHorses)
    | _, Releasing -> SwitchToMainMenu
    | _ ->
      { awardScreen with
          timer = awardScreen.timer + 1
          buttonGroup = ButtonGroup.next (lastKeyboard, keyboard) mouse gamepads awardScreen.buttonGroup }
      |> NoSwitch