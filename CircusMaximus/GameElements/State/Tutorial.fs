namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

type Tutorial = Tutorial of Player list

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Tutorial =
  let init () =
    List.init Player.numPlayers (fun i -> 
      let horses =
        { acceleration = Player.baseAcceleration * Player.unbalanceMidPoint
          topSpeed = Player.baseTopSpeed * Player.unbalanceMidPoint
          turn = Player.baseTurn * Player.unbalanceMidPoint }
      Player.init horses (new PlayerShape(0 @@ i * 100, PlayerShape.standardHeight, PlayerShape.standardHeight, 0.0)) (i + 1))
    |> Tutorial
  
  let next (lastKeyboard, keyboard) (lastGamepads, gamepads) rand settings (gameSounds: GameSounds) (Tutorial(players)) =
    let rec next players playerChariotSounds =
      match players, playerChariotSounds with
      | player :: restPlayers, playerChariotSound :: restPlayerChariotSounds ->
        let player, playerChariotSound =
          Player.next
            (lastKeyboard, keyboard) (lastGamepads, gamepads)
            rand settings Racetrack.collisionShape (Result_Poly [false; false; false; false; false; false])
            playerChariotSound player
        let restPlayers, restPlayerChariotSounds = next restPlayers restPlayerChariotSounds
        player :: restPlayers, playerChariotSound :: restPlayerChariotSounds
      | [], [] -> [], []
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    let players, playerChariotSounds = next players gameSounds.Chariots
    NoSwitch(Tutorial(players)), { gameSounds with Chariots = playerChariotSounds }