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
    mainMenuButton: Button }

/// Contains functions and constants pertaining to award screens
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AwardScreen =
  /// The default initialized award screen
  let init (settings: GameSettings) =
    { timer = 0
      mainMenuButton =
        Button.initCenter
          (settings.windowDimensions.X / 2.f @@ settings.windowDimensions.Y / 6.f)
          Button.defaultButtonSize "Exit races" }
  
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) mouse =
    { timer = awardScreen.timer + 1
      mainMenuButton = Button.next awardScreen.mainMenuButton mouse }
    |> NoSwitch