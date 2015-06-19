namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type HorseScreen(horses: Horses list, buttons: ButtonGroup) =
  member this.horses = horses
  member this.buttons = buttons
  
  interface IGameScreen with
    member this.Next deltaTime rand input = HorseScreen.next this deltaTime rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  /// Initializes a HorseScreen game state using a game fields' RNG and various Player properties
  static member init fields =
    let horses = List.init Player.numPlayers (fun i ->
      let values =
        let ump = Player.unbalanceMidPoint * 100.0 |> int
        repeat (unbalanceRandom 0 (Player.maxStatUnbalance * 100. |> int) fields.rand) [ump; ump; ump] Player.unbalanceTimes
        |> List.map (fun n -> float n / 100.0)
      { acceleration = Player.baseAcceleration * values.[0]
        topSpeed = Player.baseTopSpeed * values.[1]
        turn = Player.baseTurn * values.[2]})
    let buttons = ButtonGroup.init([ Button.initCenter (fields.settings.windowDimensions / (2 @@ 8)) Button.defaultButtonDimensions "Contine" ])
    new HorseScreen(horses, buttons), fields