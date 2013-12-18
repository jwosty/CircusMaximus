/// Contains functions and constants operating on races
module CircusMaximus.Race
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.State
open CircusMaximus.Input

let preRaceTicks, preRaceMaxCount = 200, 3
let preRaceTicksPerCount = float preRaceTicks / float preRaceMaxCount |> ceil |> int

/// The amount of time into the race that it can still be said that it has "just begun"
let midRaceBeginPeriod = preRaceTicksPerCount * 2

/// Calculates the intersections for all objects
let collideWorld players racetrackBounds = racetrackBounds :: (List.map Player.getBB players) |> Collision.collideWorld

let init rand =
  let initPlayer (bounds: PlayerShape) index =
    { motionState = Moving(0.); finishState = Racing; tauntState = None
      bounds = bounds; index = index + 1; age = 0.; effects = [];
      intersectingLines = [false; false; false; false]
      turns = if bounds.Center.Y >= Racetrack.center.Y then 0 else -1
      particles = []
      lastTurnedLeft = bounds.Center.Y >= Racetrack.center.Y }
  let x = 820.0f
  { rand = rand
    raceState = PreRace
    players =
      [
        x, 740.0f;
        x, 950.0f;
        x, 1160.0f;
        x, 1370.0f;
        x, 1580.0f;
      ] |> List.mapi (fun i (x, y) -> initPlayer (new PlayerShape(x@@y, 64.0f, 29.0f, 0.)) i)
    timer = 0 }

/// Finishes players that made the last lap
let maxTurns = 13

let nextPlayerFinish lastPlacing (player: Player) =
  match player.finishState with
  | Racing ->
    if player.turns >= maxTurns then
      {player with finishState = Finished(lastPlacing + 1)}, lastPlacing + 1
    else
      player, lastPlacing
  | Finished _ -> player, lastPlacing

let nextPlayer (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) rand assets playerIndex player collisionResult =
  let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
  let player =
    if playerIndex = 0 then Player.next (new PlayerInputState(lastKeyboard, keyboard)) player playerIndex collision (keyboard.IsKeyDown(Keys.Q)) Racetrack.center rand assets
    else
      let lastGamepad, gamepad = lastGamepads.[playerIndex - 1], gamepads.[playerIndex - 1]
      Player.next (new PlayerInputState(lastGamepad, gamepad)) player playerIndex collision (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center rand assets
  player

/// Takes a list of players and calculates the effects they have on all the other players, returning a new player list
let applyPlayerEffects players =
  players
    |> List.map (fun dst ->
      let effects =
        players
          |> List.map (fun src -> Player.applyEffects src dst)                  // Calculate player effects between each other
          |> List.reduce (fun totalEffects effects -> totalEffects @ effects)   // Collect each player's effects together
      {dst with effects = effects @ dst.effects})                               // Add the new effects to the players

/// Returns an option of a new game state (based on the input game state); None indicating that the game should stop
let next (race: Race) (lastKeyboard, keyboard) (lastGamepads, gamepads) (assets: GameContent) =
  let testDoCheer() = if race.timer = 0 then assets.CrowdCheerSound.Play() |> ignore
  let nextPlayer = nextPlayer (lastKeyboard, keyboard) (lastGamepads, gamepads) race.rand assets
  if keyboard.IsKeyDown(Keys.Escape) then
    None  // Indicate that we want to exit
  else
    match race.raceState with
    | PreRace ->
      if race.timer >= preRaceTicks then
        Some({rand = race.rand; raceState = DynamicRace(MidRace(0)); players = race.players; timer = 0})
      else
        Some({race with timer = race.timer + 1})
    | DynamicRace dynamicRace ->
      testDoCheer()
      let playerCollisions = collideWorld race.players Racetrack.collisionBounds |> List.tail
      // Dynamic races, by definition, require player updates, but player placings are only used before the race is over. These two conditions
      // mean that Player.next can't do placings, so we have to do it down there somewhere.
      let dynamicRaceState, players =
        match dynamicRace with
        | MidRace oldLastPlacing ->
          let _, players, lastPlacing =
            // Acts as a simeltanious map and fold (map for player updating, fold for keeping track of the last placing)
            List.foldBack2
              (fun player collision (i, players, lastPlacing) ->
                let player = nextPlayer i player collision
                let player, newLastPlacing = nextPlayerFinish lastPlacing player
                i - 1, (player :: players), newLastPlacing)
              race.players playerCollisions (race.players.Length - 1, [], oldLastPlacing)
          if oldLastPlacing <> lastPlacing then assets.CrowdCheerSound.Play() |> ignore // Congradulate the player for finishing in the top 3
          
          // The race is over as soon as the last player finishes
          if lastPlacing = players.Length
            then PostRace, players
            else MidRace(lastPlacing), players
        // No player placings
        | PostRace -> PostRace, List.mapi2 nextPlayer race.players playerCollisions
      let players = applyPlayerEffects players
      Some({rand = race.rand; raceState = DynamicRace(dynamicRaceState); players = players; timer = race.timer + 1})