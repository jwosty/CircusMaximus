namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type MainMenu = { playButton: Button; quitButton: Button }

/// Contains functions and constants pertaining to main menus
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MainMenu =
  let init (settings: GameSettings) =
    { playButton =
        Button.initCenter
          (settings.windowDimensions * (0.5 @@ 0.333))
          Button.defaultButtonSize "Principio"
      quitButton =
        Button.initCenter
          (settings.windowDimensions * (0.5 @@ 0.666))
          Button.defaultButtonSize "Desero" }
  
  /// Updates the main menu
  let next (mainMenu: MainMenu) (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) =
    match mainMenu.playButton.buttonState, mainMenu.quitButton.buttonState with
    | _, Releasing -> NativeExit
    | Releasing, _ -> SwitchToRaces
    | _, _ ->
      NoSwitch(
        { mainMenu with
            playButton = Button.next mainMenu.playButton mouse
            quitButton = Button.next mainMenu.quitButton mouse })