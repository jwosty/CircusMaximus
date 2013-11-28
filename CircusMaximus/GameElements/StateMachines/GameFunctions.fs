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

/// Calculates the intersections for all objects
let collideWorld players racetrackBounds = racetrackBounds :: (List.map Player.getBB players) |> Collision.collideWorld

/// Updates players (collision and input)
let updatePlayers (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) players collisions assets =
  List.mapi2
    (fun i player collisionResult ->
      let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen... It's probably a bug!"
      let otherPlayers = List.removeIndex i players // eww... better / more efficient way to do this?
      let player =
        if i = 0 then Player.next (new PlayerInputState(lastKeyboard, keyboard)) player i collision (keyboard.IsKeyDown(Keys.Q)) Racetrack.center assets
        else
          let lastGamepad, gamepad = lastGamepads.[i - 1], gamepads.[i - 1]
          Player.next (new PlayerInputState(lastGamepad, gamepad)) player i collision (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center assets
      player)
    players
    (collisions |> List.tail)

/// Returns an option of a new game state (based on the input game state); None indicating
/// that the game should stop
let nextRace (race: Race) (lastKeyboard, keyboard: KeyboardState) (lastGamepads: GamePadState list, gamepads: GamePadState list) (assets: GameContent) =
  let testDoCheer() = if race.timer = 0 then assets.CrowdCheerSound.Play() |> ignore
  if keyboard.IsKeyDown(Keys.Escape) then
    None  // Indicate that we want to exit
  else
    match race.raceState with
    | PreRace ->
      if race.timer >= preRaceTicks then
        Some({raceState = DynamicRace(MidRace(0)); players = race.players; timer = 0})
      else
        Some({race with timer = race.timer + 1})
    | DynamicRace dynamicRace ->
      testDoCheer()
      let collisions = collideWorld race.players Racetrack.collisionBounds
      let players = updatePlayers (lastKeyboard, keyboard) (lastGamepads, gamepads) race.players collisions assets
      let dynamicRaceState =
        match dynamicRace with
        | MidRace oldLastPlacing ->
          let lastPlacing =
            List.fold2
              (fun lastPlacing oldPlayer player -> if Player.justFinished oldPlayer player then lastPlacing + 1 else lastPlacing)
              oldLastPlacing race.players players
          if oldLastPlacing <> lastPlacing then assets.CrowdCheerSound.Play() |> ignore  // Congradulate the player for finishing in the top 3
          if lastPlacing = race.players.Length then PostRace else MidRace(lastPlacing)  
        | PostRace -> PostRace
      Some({raceState = DynamicRace(dynamicRaceState); players = players; timer = race.timer + 1})