namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ButtonGroup =
  /// Initializes a button group
  let init buttons =
    { buttons = buttons
      selected = 0 }
  
  /// Returns the next button group state, updating all children buttons
  let next (lastKeyboard, keyboard) mouse gamepads buttonGroup =
    let keyJustPressed = keyJustPressed (lastKeyboard, keyboard)
    let selected =
      // Selection from arrow keys
      let kDirection = 
        match keyJustPressed Keys.Up, keyJustPressed Keys.Down with
        | true, false -> -1
        | false, true -> 1
        | _ -> 0
      // Selection from each gamepad's left thumbstick
      List.fold
        (fun direction (gamepad: GamePadState) ->
          direction + (-gamepad.ThumbSticks.Left.Y |> round |> int))
        0 gamepads
      // Combine keyboard arrows and gamepad thumbsticks
      + kDirection
      // Change from current selection
      + buttonGroup.selected
      |> clamp 0 (buttonGroup.buttons.Length - 1)
    { buttonGroup with
        selected = selected
        buttons =
          buttonGroup.buttons |> List.mapi (fun i button ->
            match i = buttonGroup.selected, button.isSelected with
            | true, false -> { button with isSelected = true }
            | false, true -> { button with isSelected = false }
            | _ -> button
            |> Button.next mouse keyboard gamepads) }
  
  let findByLabel (buttonGroup: ButtonGroup) label =
    List.find (fun (button: Button) -> button.label = label) buttonGroup.buttons
  
  let buttonState buttonGroup label =
    (findByLabel buttonGroup label).buttonState