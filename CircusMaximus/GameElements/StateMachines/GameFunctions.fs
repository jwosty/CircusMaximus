/// Contains functions and constants pertaining to players
[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module CircusMaximus.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Game
open CircusMaximus.Player
open CircusMaximus.Input

let preRaceTicks, preRaceMaxCount = 200, 3
let preRaceTicksPerCount = float preRaceTicks / float preRaceMaxCount |> ceil |> int

/// The amount of time into the race that it can still be said that it has "just begun"
let midRaceBeginPeriod = preRaceTicksPerCount * 2

let baseRaceData = function
  | PreRace raceData -> raceData
  | MidRace(raceData, _) -> raceData
  | PostRace raceData -> raceData

let nextMovingRace (lastKeyboard: KeyboardState, keyboard: KeyboardState) (lastGamepads: GamePadState list, gamepads: GamePadState list) players raceLastPlacing timer assets =
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
        let player =
          if i = 0 then nextPlayer (new PlayerInputState(lastKeyboard, keyboard)) player i collision raceLastPlacing (keyboard.IsKeyDown(Keys.Q)) Racetrack.center assets
          else
            let lastGamepad, gamepad = lastGamepads.[i - 1], gamepads.[i - 1]
            nextPlayer (new PlayerInputState(lastGamepad, gamepad)) player i collision raceLastPlacing (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center assets
        // TODO: Not functional. Fix it!!
        match ((commonPlayerData player).placing)
          with | Some placing -> (lastPlacing := placing) | None -> ()
        player)
      players
      (collisions |> List.tail)
  // Return the new and improved game state
  Some(CommonRaceData(updatedPlayers, timer + 1), !lastPlacing)

let nextMidRace (lastKeyboard, keyboard) (lastGamepads, gamepads) players raceLastPlacing timer assets =
  let result = nextMovingRace (lastKeyboard, keyboard) (lastGamepads, gamepads) players raceLastPlacing timer assets
  match result with
  | Some(nextState, lastPlacing) -> Some(MidRace(nextState, lastPlacing))
  | None -> None

let nextPostRace (lastKeyboard, keyboard) (lastGamepads, gamepads) players raceLastPlacing timer assets =
  let result = nextMovingRace (lastKeyboard, keyboard) (lastGamepads, gamepads) players raceLastPlacing timer assets
  match result with
  | Some(nextState, _) -> Some(PostRace(nextState))
  | None -> None

/// Returns an option of a new game state (based on the input game state); None indicating
/// that the game should stop
let nextGame gameState (lastKeyboard, keyboard: KeyboardState) (lastGamepad, gamepad) (assets: GameContent) =
  if keyboard.IsKeyDown(Keys.Escape) then
    None  // Indicate that we want to exit
  else
    match gameState with
    | PreRace raceData ->
      if raceData.timer >= preRaceTicks then
        Some(MidRace(CommonRaceData(raceData.players, 0), 0))
      else
        Some(PreRace(CommonRaceData(raceData.players, raceData.timer + 1)))
    | MidRace(raceData, lastPlacing) ->
      if raceData.timer = 0 then assets.CrowdCheerSound.Play() |> ignore
      nextMidRace(lastKeyboard, keyboard) (lastGamepad, gamepad) raceData.players lastPlacing raceData.timer assets
    | PostRace raceData ->
      nextPostRace (lastKeyboard, keyboard) (lastGamepad, gamepad) raceData.players (raceData.players.Length - 1) raceData.timer assets