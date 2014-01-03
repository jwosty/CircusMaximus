namespace CircusMaximus.State
open System

type ScreenStatus<'a> =
  /// Continue updating as usual
  | NoSwitch of 'a
  /// Exit and begin a new race
  | SwitchToRaces
  /// Return to the main menu
  | SwitchToMainMenu
  /// Quit CircusMaximus
  | NativeExit

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ScreenStatus =
  /// Converts a ScreenStatus<'a> into a ScreenStatus<'b> using the given mapping.
  let map mapping screenStatus =
    match screenStatus with
    | NoSwitch x -> NoSwitch(mapping x)
    | SwitchToRaces -> SwitchToRaces
    | SwitchToMainMenu -> SwitchToMainMenu
    | NativeExit -> NativeExit