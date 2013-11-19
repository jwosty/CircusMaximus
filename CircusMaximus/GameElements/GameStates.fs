module CircusMaximus.State.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision

type MidRaceData =
  struct
    val players: Player.Player list
    val timer: int
    val lastPlacing: int
    
    new(players, timer, lastPlacing) =  { players = players; timer = timer; lastPlacing = lastPlacing }
  end

type Game = | MidRace of MidRaceData

/// Update a gameState, returning an option of the new state; the Some case here represents that the game
/// shall continue and None indicating that the whole game should stop
let update gameState =
  match gameState with
    | MidRace raceData ->
        let keyboard = Keyboard.GetState()
        if keyboard.IsKeyDown(Keys.Escape) then
          None  // Indicate that we want to exit
        else
          // A list of collision results (more like intersection results)
          let collisions =
            (Racetrack.collisionBounds
              :: (raceData.players |> List.map Player.playerBB))
              |> Collision.collideWorld
          let lastPlacing = ref raceData.lastPlacing
          // Update the players (collision and input)
          let updatedPlayers =
            List.mapi2
              (fun i player collisionResult ->
                let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen... It's probably a bug!"
                let otherPlayers = List.removeIndex i raceData.players // eww... better / more efficient way to do this?
                let player, p =
                  if i = 0 then Player.update (Player.getPowerTurnFromKeyboard keyboard) player collision raceData.lastPlacing (keyboard.IsKeyDown(Keys.Q)) Racetrack.center
                  else
                    let gamepad = GamePad.GetState(enum <| i - 1)
                    Player.update (Player.getPowerTurnFromGamepad gamepad) player collision raceData.lastPlacing (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center
                // TODO: Not functional. Fix it!!
                match p with | Some placing -> (lastPlacing := placing) | None -> ()
                player)
              raceData.players
              (collisions |> List.tail)
          // Return the new and improved game state
          Some(MidRace(MidRaceData(updatedPlayers, raceData.timer + 1, !lastPlacing)))