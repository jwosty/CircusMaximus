module CircusMaximus.State.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision
open CircusMaximus.PlayerInput
open CircusMaximus.State.Player

type PreRaceData =
  struct
    val players: Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

type MidRaceData =
  struct
    val players: Player list
    val timer: int
    val lastPlacing: int
    
    new(players, timer, lastPlacing) = { players = players; timer = timer; lastPlacing = lastPlacing }
  end

type PostRaceData =
  struct
    val players: Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

/// Game state machine
type GameState =
  | PreRace of PreRaceData
  | MidRace of MidRaceData
  | PostRace of PostRaceData

type Game = GameState

let preRaceTicks, preRaceMaxCount = 200, 3
let preRaceTicksPerCount = float preRaceTicks / float preRaceMaxCount |> ceil |> int

/// The amount of time into the race that it can still be said that it has "just begun"
let midRaceBeginPeriod = preRaceTicksPerCount * 2

let playerQuantity = function
  | PreRace raceData -> raceData.players.Length
  | MidRace raceData -> raceData.players.Length
  | PostRace raceData -> raceData.players.Length

let updateMovingRace (lastKeyboard: KeyboardState, keyboard: KeyboardState) (lastGamepads: GamePadState list, gamepads: GamePadState list) players raceLastPlacing timer assets =
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
          if i = 0 then Player.update (new PlayerInputState(lastKeyboard, keyboard)) player i collision raceLastPlacing (keyboard.IsKeyDown(Keys.Q)) Racetrack.center assets
          else
            let lastGamepad, gamepad = lastGamepads.[i - 1], gamepads.[i - 1]
            Player.update (new PlayerInputState(lastGamepad, gamepad)) player i collision raceLastPlacing (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center assets
        // TODO: Not functional. Fix it!!
        match p with | Some placing -> (lastPlacing := placing) | None -> ()
        player)
      players
      (collisions |> List.tail)
  // Return the new and improved game state
  Some(MidRace(MidRaceData(updatedPlayers, timer + 1, !lastPlacing)))

/// Updates a gameState, returning an option of the new state; the Some case here represents that the game
/// shall continue and None indicating that the whole game should stop
let update gameState (lastKeyboard, keyboard: KeyboardState) (lastGamepad, gamepad) (assets: GameContent) =
  if keyboard.IsKeyDown(Keys.Escape) then
    None  // Indicate that we want to exit
  else
    match gameState with
    | PreRace raceData ->
      if raceData.timer >= preRaceTicks then
        Some(MidRace(MidRaceData(raceData.players, 0, 0)))
      else
        Some(PreRace(PreRaceData(raceData.players, raceData.timer + 1)))
    | MidRace raceData ->
      if raceData.timer = 0 then assets.CrowdCheerSound.Play() |> ignore
      updateMovingRace (lastKeyboard, keyboard) (lastGamepad, gamepad) raceData.players raceData.lastPlacing raceData.timer assets
    | PostRace raceData -> updateMovingRace (lastKeyboard, keyboard) (lastGamepad, gamepad) raceData.players (raceData.players.Length - 1) raceData.timer assets