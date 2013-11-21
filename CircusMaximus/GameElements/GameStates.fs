module CircusMaximus.State.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision

type PreRaceData =
  struct
    val players: Player.Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

type MidRaceData =
  struct
    val players: Player.Player list
    val timer: int
    val lastPlacing: int
    
    new(players, timer, lastPlacing) = { players = players; timer = timer; lastPlacing = lastPlacing }
  end

type PostRaceData =
  struct
    val players: Player.Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

type MovingRace =
  | MidRace of MidRaceData
  | PostRace of PostRaceData

type Game =
  | PreRace of PreRaceData
  | MidRace of MidRaceData
  | PostRace of PostRaceData

let preRaceTicks, preRaceMaxCount = 200, 3
let preRaceTicksPerCount = float preRaceTicks / float preRaceMaxCount |> ceil |> int

/// The amount of time into the race that it can still be said that it has "just begun"
let midRaceBeginPeriod = preRaceTicksPerCount * 2

let updateMovingRace (keyboard: KeyboardState) (getGamepad: PlayerIndex -> _) players raceLastPlacing timer =
  // A list of collision results (more like intersection results)
  let collisions =
    (Racetrack.collisionBounds
      :: (players |> List.map Player.playerBB))
      |> Collision.collideWorld
  let lastPlacing = ref raceLastPlacing
  // Update the players (collision and input)
  let updatedPlayers =
    List.mapi2
      (fun i player collisionResult ->
        let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen... It's probably a bug!"
        let otherPlayers = List.removeIndex i players // eww... better / more efficient way to do this?
        let player, p =
          if i = 0 then Player.update (Player.getPowerTurnFromKeyboard keyboard) player collision raceLastPlacing (keyboard.IsKeyDown(Keys.Q)) Racetrack.center
          else
            let gamepad = getGamepad(enum <| i - 1)
            Player.update (Player.getPowerTurnFromGamepad gamepad) player collision raceLastPlacing (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center
        // TODO: Not functional. Fix it!!
        match p with | Some placing -> (lastPlacing := placing) | None -> ()
        player)
      players
      (collisions |> List.tail)
  // Return the new and improved game state
  Some(MidRace(MidRaceData(updatedPlayers, timer + 1, !lastPlacing)))

/// Updates a gameState, returning an option of the new state; the Some case here represents that the game
/// shall continue and None indicating that the whole game should stop
let update gameState (keyboard: KeyboardState) (getGamepad: PlayerIndex -> _) =
  if keyboard.IsKeyDown(Keys.Escape) then
    None  // Indicate that we want to exit
  else
    match gameState with
    | PreRace raceData ->
      if raceData.timer >= preRaceTicks then
        Some(MidRace(MidRaceData(raceData.players, 0, 0)))
      else
        Some(PreRace(PreRaceData(raceData.players, raceData.timer + 1)))
    | MidRace raceData -> updateMovingRace (Keyboard.GetState()) GamePad.GetState raceData.players raceData.lastPlacing raceData.timer
    | PostRace raceData -> updateMovingRace (Keyboard.GetState()) GamePad.GetState raceData.players (raceData.players.Length - 1) raceData.timer