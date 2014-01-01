namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type Screen =
  /// Two buttons: Principio (play) and Desero (quit)
  | MainMenu of Button * Button

type status<'a> =
  /// Continue updating the screen as usual
  | Keep of 'a
  /// Exit the screen and start a race
  | ExitToRaces
  /// Quit CircusMaximus
  | NativeExit

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Screen =
  let initMainMenu (settings: GameSettings) =
    MainMenu(
      Button.initCenter
        (settings.windowDimensions * (0.5 @@ 0.333))
        Button.defaultButtonSize "Principio",
      Button.initCenter
        (settings.windowDimensions * (0.5 @@ 0.666))
        Button.defaultButtonSize "Desero")
  
  let next screen (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) =
    match screen with
    | MainMenu(playButton, quitButton) ->
      match playButton.buttonState, quitButton.buttonState with
      | _, Releasing -> NativeExit
      | Releasing, _ -> ExitToRaces
      | _, _ -> Keep(MainMenu(Button.next playButton mouse, Button.next quitButton mouse))