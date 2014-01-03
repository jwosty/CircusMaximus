namespace CircusMaximus.State
open System

type ScreenStatus<'a> =
  /// Continue updating as usual
  | NoSwitch of 'a
  /// Exit and begin a new race
  | SwitchToRaces
  /// Quit CircusMaximus
  | NativeExit