namespace CircusMaximus.State
open System

type ScreenStatus<'a> =
  /// Continue updating as usual
  | NoSwitch of 'a
  | SwitchToMainMenu
  | SwitchToHorseScreen
  | SwitchToRaces
  | SwitchToAwards
  /// Quit CircusMaximus
  | NativeExit

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ScreenStatus =
  /// Converts a ScreenStatus<'a> into a ScreenStatus<'b> using the given mapping.
  let map mapping screenStatus =
    match screenStatus with
    | NoSwitch x -> NoSwitch(mapping x)
    | SwitchToHorseScreen -> SwitchToHorseScreen
    | SwitchToMainMenu -> SwitchToMainMenu
    | SwitchToRaces -> SwitchToRaces
    | SwitchToAwards -> SwitchToAwards
    | NativeExit -> NativeExit