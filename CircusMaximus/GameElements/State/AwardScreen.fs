namespace CircusMaximus.State
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
    mainMenuButton: Button
    continueButton: Button }

/// Contains functions and constants pertaining to award screens
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AwardScreen =
  /// The default initialized award screen
  let init (settings: GameSettings) (playerDataAndWinnings: (PlayerData * int) list) =
    let x = settings.windowDimensions.X / 2.f
    let y8 = settings.windowDimensions.Y / 10.f
    // Player data and their winnings paired together conveniently
    (*
    let playerDataAndWinnings =
      List.init players.Length
        (fun i ->
          let number = i + 1
          let winnings =
            match (Player.findByNumber number players).finishState with
            | Finished placing -> PlayerData.playerWinnings number
            | Racing _ -> 0
          PlayerData.findByNumber number playerData, winnings)
    *)
    // The new playerData that the game should now use
    let newPlayerData =
      playerDataAndWinnings |> List.map
        (fun (playerData, winnings) -> { playerData with coinBalance = playerData.coinBalance + winnings })
    
    { timer = 0
      playerDataAndWinnings = playerDataAndWinnings
      continueButton =
        Button.initCenter
          (x @@ y8)
          Button.defaultButtonSize "Continue"
      mainMenuButton =
        Button.initCenter
          (x @@ y8 * 2.f)
          Button.defaultButtonSize "Exit races" },
    newPlayerData
  
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) mouse =
    match awardScreen.continueButton.buttonState, awardScreen.mainMenuButton.buttonState with
    | Releasing, _ -> SwitchToRaces
    | _, Releasing -> SwitchToMainMenu
    | _ ->
      { awardScreen with
          timer = awardScreen.timer + 1
          continueButton = Button.next awardScreen.continueButton mouse
          mainMenuButton = Button.next awardScreen.mainMenuButton mouse } |> NoSwitch