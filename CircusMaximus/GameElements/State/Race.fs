namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type LastPlacing = int

type RaceState = | PreRace | MidRace of LastPlacing | PostRace

type Race = { raceState: RaceState; players: Player list; timer: int }

/// Contains functions and constants operating on races
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Race =
  let preRaceTicks, preRaceMaxCount = 200, 3
  let preRaceTicksPerCount = float preRaceTicks / float preRaceMaxCount |> ceil |> int
  
  /// The amount of time into the race that it can still be said that it has "just begun"
  let midRaceBeginPeriod = preRaceTicksPerCount * 2
  
  /// Calculates the intersections for all objects
  let collideWorld players racetrackBounds = racetrackBounds :: (List.map Player.getBB players) |> Collision.collideWorld
  
  /// Finishes players that made the last lap
  let maxTurns = 13

  let init () =
    let x = 820.0f
    { raceState = PreRace
      players =
        [
          x, 740.0f;
          x, 950.0f;
          x, 1160.0f;
          x, 1370.0f;
          x, 1580.0f;
        ] |> List.mapi (fun i (x, y) -> Player.init (new PlayerShape(x@@y, 64.0f, 29.0f, 0.)) (i + 1))
      timer = 0 }
  
  let findPlayerByNumber number (race: Race) = race.players |> List.find (fun p -> p.number = number)
  
  let nextPlayerFinish lastPlacing (player: Player) =
    match player.finishState with
    | Racing ->
      if player.turns >= maxTurns then
        {player with finishState = Finished(lastPlacing + 1)}, lastPlacing + 1
      else
        player, lastPlacing
    | Finished _ -> player, lastPlacing
  
  let nextPlayer (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) rand player collisionResult =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player =
      if player.number = 1 then
        Player.next
          (PlayerInput.initFromKeyboard (lastKeyboard, keyboard) PlayerInput.maxTurn PlayerInput.maxSpeed)
          player collision Racetrack.center rand
      else
        let lastGamepad, gamepad = lastGamepads.[player.number - 2], gamepads.[player.number - 2]
        Player.next
          (PlayerInput.initFromGamepad (lastGamepad, gamepad) PlayerInput.maxTurn PlayerInput.maxSpeed)
          player collision Racetrack.center rand
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
  
  let next (race: Race) (lastKeyboard, keyboard) (lastGamepads, gamepads) rand gameSound =
    let nextPlayer = nextPlayer (lastKeyboard, keyboard) (lastGamepads, gamepads) rand
    match race.raceState with
    | PreRace ->
      if race.timer >= preRaceTicks then
        {raceState = MidRace(0); players = race.players; timer = 0}, gameSound
      else
        {race with timer = race.timer + 1}, gameSound
    
    | _ ->
      let playerCollisions = collideWorld race.players Racetrack.collisionBounds |> List.tail
      // "Dynamic" races require player updates, but player placings are only used before the race is over. These two conditions
      // mean that Player.next can't do placings, so we have to do it down there somewhere.
      let raceState, players, newGameSound =
        match race.raceState with
        | MidRace oldLastPlacing ->
          let _, players, lastPlacing =
            // Acts as a simeltanious map and fold (map for player updating, fold for keeping track of the last placing)
            List.foldBack2
              (fun player collision (i, players, lastPlacing) ->
                let player = nextPlayer player collision
                let player, newLastPlacing = nextPlayerFinish lastPlacing player
                i - 1, (player :: players), newLastPlacing)
              race.players playerCollisions (race.players.Length - 1, [], oldLastPlacing)
          //if oldLastPlacing <> lastPlacing then assets.CrowdCheerSound.Play() |> ignore // Congratulate the player for finishing in the top 3
          let newGameSound =
            if oldLastPlacing <> lastPlacing || race.timer = 0   // Congratulate the player for finishing in the top 3, or cheer to start off the race
            then { gameSound with CrowdCheer = Playing }
            else gameSound
          // The race is over as soon as the last player finishes
          if lastPlacing = players.Length
            then PostRace, players, newGameSound
            else MidRace(lastPlacing), players, newGameSound
        // No player placings
        | PostRace -> PostRace, List.map2 nextPlayer race.players playerCollisions, gameSound
      let players = applyPlayerEffects players
      {raceState = raceState; players = players; timer = race.timer + 1}, newGameSound