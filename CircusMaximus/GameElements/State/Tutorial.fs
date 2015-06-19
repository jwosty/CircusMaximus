namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Collision
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

type Tutorial(players) =
  member this.players = players
  
  interface IGameScreen with
    member this.Next deltaTime rand input = Tutorial.next this deltaTime rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  /// Initialize a Tutorial screen
  static member init () =
    let players =
      List.init Player.numPlayers (fun i ->
        let horses =
          { acceleration = Player.baseAcceleration * Player.unbalanceMidPoint
            topSpeed = Player.baseTopSpeed * Player.unbalanceMidPoint
            turn = Player.baseTurn * Player.unbalanceMidPoint }
        Player.init horses (new PlayerShape(0<px> @@ i * 100<px>, PlayerShape.standardDimensions, 0.0<r>)) (i + 1))
    new Tutorial(players)