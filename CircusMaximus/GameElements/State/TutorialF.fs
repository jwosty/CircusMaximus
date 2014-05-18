namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision
open CircusMaximus.Types

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Tutorial =
  let next fields (lastKeyboard, keyboard) (lastGamepads, gamepads) (gameSounds: GameSounds) (tutorial: Tutorial) =
    let rec next players playerChariotSounds =
      match players, playerChariotSounds with
      | player :: restPlayers, playerChariotSound :: restPlayerChariotSounds ->
        let player, playerChariotSound =
          Player.next
            fields (lastKeyboard, keyboard) (lastGamepads, gamepads)
            Racetrack.collisionShape.RespawnPath (Result_Poly [false; false; false; false; false; false])
            playerChariotSound player
        let restPlayers, restPlayerChariotSounds = next restPlayers restPlayerChariotSounds
        player :: restPlayers, playerChariotSound :: restPlayerChariotSounds
      | [], [] -> [], []
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    let players, playerChariotSounds = next tutorial.players gameSounds.Chariots
    let t = tutorial :> IGameScreen
    
    new Tutorial(players), { gameSounds with Chariots = playerChariotSounds }