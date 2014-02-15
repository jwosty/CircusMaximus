namespace CircusMaximus.State
open System

type ScreenStatus<'a> =
  /// Continue updating as usual
  | NoSwitch of 'a
  | SwitchToMainMenu
  | SwitchToTutorial
  | SwitchToHorseScreen
  | SwitchToRaces of Horses list
  | SwitchToAwards
  /// Quit CircusMaximus
  | NativeExit

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ScreenStatus =
  /// Converts a ScreenStatus<'a> into a ScreenStatus<'b> using the given mapping.
  let map mapping screenStatus =
    match screenStatus with
    | NoSwitch x -> NoSwitch(mapping x)
    | SwitchToMainMenu -> SwitchToMainMenu
    | SwitchToTutorial -> SwitchToTutorial
    | SwitchToHorseScreen -> SwitchToHorseScreen
    | SwitchToRaces(playerHorses) -> SwitchToRaces(playerHorses)
    | SwitchToAwards -> SwitchToAwards
    | NativeExit -> NativeExit