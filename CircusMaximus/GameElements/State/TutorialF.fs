namespace CircusMaximus.Functions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision
open CircusMaximus.Types

module Tutorial =
  let next (tutorial: Tutorial) deltaTime fields input =
    let rec next players chariotSounds =
      match players, chariotSounds with
      | player :: restPlayers, playerChariotSound :: restPlayerChariotSounds ->
        let player, playerChariotSound =
          Player.next
            fields input
            Racetrack.collisionShape.RespawnPath (Result_Poly [false; false; false; false; false; false])
            playerChariotSound player
        let restPlayers, restPlayerChariotSounds = next restPlayers restPlayerChariotSounds
        player :: restPlayers, playerChariotSound :: restPlayerChariotSounds
      | [], [] -> [], []
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    let players, chariotSounds = next tutorial.players fields.sounds.Chariots
    Some(new Tutorial(players) :> IGameScreen, { fields with sounds = { fields.sounds with Chariots = chariotSounds } } )