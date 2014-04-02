namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

type Tutorial =
  | Tutorial of Player list

  /// Initialize a Tutorial screen
  static member init () =
    List.init Player.numPlayers (fun i -> 
      let horses =
        { acceleration = Player.baseAcceleration * Player.unbalanceMidPoint
          topSpeed = Player.baseTopSpeed * Player.unbalanceMidPoint
          turn = Player.baseTurn * Player.unbalanceMidPoint }
      Player.init horses (new PlayerShape(0 @@ i * 100, PlayerShape.standardHeight, PlayerShape.standardHeight, 0.0)) (i + 1))
    |> Tutorial