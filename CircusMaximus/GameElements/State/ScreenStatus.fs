namespace CircusMaximus.Types
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