/// TODO: Change time expressed in frames to time expressed in seconds. Can be done by using the ideal FPS, which is 60.<fr/s>, as the conversion value
/// Conversion units for pixels: 35.<px>/8<ft>, or about 35.<px>/2.4384<m> (14.35<px/m>)

namespace CircusMaximus.Types.UnitNames
  /// Unit of length currently used in the game, will be replaced by meters
  type [<Measure>] pixel
  /// Unit of time currently used in the game, will be replaced by seconds
  type [<Measure>] frame
  /// Unit of angles based on pi
  type [<Measure>] radian
  /// Unit of time equal to 1/1000 second
  type [<Measure>] millisecond

namespace CircusMaximus.Types.UnitSymbols
  open CircusMaximus.Types.UnitNames
  /// Unit of length currently used in the game, will be replaced by meters
  type [<Measure>] px = pixel
  /// Unit of time currently used in the game, will be replaced by seconds
  type [<Measure>] fr = frame
  /// Unit of angles based on pi
  type [<Measure>] r = radian
  /// Unit of time equal 1/1000 second
  type [<Measure>] ms = millisecond

namespace CircusMaximus.Types
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open CircusMaximus.Types.UnitSymbols

  module UnitConversions =
    let msPerS = 1000.<ms/s>