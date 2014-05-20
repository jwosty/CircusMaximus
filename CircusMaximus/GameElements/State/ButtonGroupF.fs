namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types

module ButtonGroup =
  /// Returns the next button group state, updating all children buttons
  let next buttonGroup input =
    let keyJustPressed = keyJustPressed (input.lastKeyboard, input.keyboard)
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
        0 input.gamepads
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
            |> Button.next input) }
  
  let findByLabel (buttonGroup: ButtonGroup) label =
    List.find (fun (button: Button) -> button.label = label) buttonGroup.buttons
  
  let buttonState buttonGroup label =
    (findByLabel buttonGroup label).buttonState