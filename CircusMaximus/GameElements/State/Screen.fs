namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type Screen =
  | MainMenu of Button  // For now, the main menu only has a single button, the play button

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Screen =
  let next screen (lastMouse, mouse) (lastKeyboard, keyboard: KeyboardState) (lastGamepads, gamepad) =
    match screen with
    | MainMenu playButton ->
      match playButton.buttonState with
      | Releasing -> None
      | _ -> Some(MainMenu(Button.next playButton mouse))