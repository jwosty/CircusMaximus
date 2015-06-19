/// TODO: Change time expressed in frames to time expressed in seconds. Can be done by using the game's average FPS (float<fr/s>) (on my computer) as the conversion unit

namespace CircusMaximus.Types.UnitNames
  /// Unit of length currently used in the game, will be replaced by meters
  type [<Measure>] pixel
  /// Unit of time currently used in the game, will be replaced by seconds
  type [<Measure>] frame
  /// Unit of angles based on pi
  type [<Measure>] radian

namespace CircusMaximus.Types.UnitSymbols
  /// Unit of length currently used in the game, will be replaced by meters
  type [<Measure>] px = CircusMaximus.Types.UnitNames.pixel
  /// Unit of time currently used in the game, will be replaced by seconds
  type [<Measure>] fr = CircusMaximus.Types.UnitNames.frame
  /// Unit of angles based on pi
  type [<Measure>] r = CircusMaximus.Types.UnitNames.radian