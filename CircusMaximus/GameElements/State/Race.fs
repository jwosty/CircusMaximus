namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type LastPlacing = int

type RaceState = | PreRace | MidRace of LastPlacing | PostRace of Button

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
  
  let initPostRaceState defaultButtonSize (settings: GameSettings) =
    PostRace(Button.initCenter
      (settings.windowDimensions.X / 2.f @@ settings.windowDimensions.Y / 6.f * 1.f)
      defaultButtonSize "Continue")
  
  let init settings =
    let playerY n = (n - 1) * 210 + 740 |> float32
    { raceState = PreRace
      players =
        [ for n in 1..5 ->
          let basePlayer = Player.init (new PlayerShape(820.f @@ playerY n, 64.0f, 29.0f, 0.)) n
          if n < 2
            then basePlayer
            else { basePlayer with finishState = Finished(Player.numPlayers - n + 1) } ]
      timer = 0 }
  
  /// Takes a list of players and calculates the effects they have on all the other players, returning a new player list
  let applyPlayerEffects players =
    players
      |> List.map (fun dst ->
        let effects =
          players
            |> List.map (fun src -> Player.applyEffects src dst)                  // Calculate player effects between each other
            |> List.reduce (fun totalEffects effects -> totalEffects @ effects)   // Collect each player's effects together
        { dst with effects = effects @ dst.effects })                             // Add the new effects to the players
  
  let nextPlayers playerMapper latestPlacing playerCollisions playerChariotSounds players =
    let rec nextPlayers updatedPlayers lastPlacing updatedChariotSounds
                        (nextPlayerFunction: Collision.CollisionResult -> SoundState -> Player -> Player * SoundState)
                        playerCollisions playerChariotSounds players =
      match playerCollisions, playerChariotSounds, players with
      | playerCollision :: restPlayerCollisions,
        playerChariotSound :: restPlayerChariotSounds,
        player :: restPlayers ->
          let player, nextPlayerChariotSound = nextPlayerFunction playerCollision playerChariotSound player
          let player, lastPlacing = Player.nextPlayerFinish maxTurns lastPlacing player
          nextPlayers
            (updatedPlayers @ [player]) lastPlacing (updatedChariotSounds @ [nextPlayerChariotSound])
            nextPlayerFunction restPlayerCollisions restPlayerChariotSounds restPlayers
      | [], [], [] -> updatedPlayers, lastPlacing, updatedChariotSounds
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    // Not curried because it gives an ugly function signature
    nextPlayers [] latestPlacing [] playerMapper playerCollisions playerChariotSounds players
  
  /// A quick 'n dirty function to find the last player placing so we know what to use next when someone
  /// else finishes.
  let getLastPlacing players =
    List.fold
      (fun lastPlacing (player: Player) -> if player.finished then lastPlacing + 1 else lastPlacing)
      0 players
  
  /// Returns the next race state. 
  let next (race: Race) mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) rand gameSound (settings: GameSettings) =
    let nextPlayer = Player.next (lastKeyboard, keyboard) (lastGamepads, gamepads) rand
    match race.raceState with
    | PreRace ->
      let race, gameSounds =
        if race.timer >= preRaceTicks then
          // Begin the race when it's time
          { raceState = MidRace(getLastPlacing race.players)  // In case we needed to hard-code some players to start into the pre-race state; this should normally return 0
            players = race.players
            timer = 0 },
          { gameSound with CrowdCheer = Playing 1 }   // The crowd gets exited when the race begins
        else
          { race with timer = race.timer + 1 },   // Simply increment the timer until the race starts
          gameSound   // No sounds here
      NoSwitch(race), gameSounds
    
    | _ ->
      let playerCollisions = collideWorld race.players Racetrack.collisionBounds |> List.tail
      // "Dynamic" races require player updates, but player placings are only used before the race is over. These two conditions
      // mean that Player.next can't do placings, so we have to do it down there somewhere.
      let raceStatus, raceState, players, gameSound =
        match race.raceState with
        | MidRace oldLastPlacing ->
          let players, latestPlacing, playerChariotSounds = nextPlayers nextPlayer oldLastPlacing playerCollisions gameSound.Chariots race.players
          let newGameSound =
            { CrowdCheer =
                if oldLastPlacing <> latestPlacing   // Congratulate the player for finishing in the top 3
                then Playing 1
                else gameSound.CrowdCheer
              Chariots = playerChariotSounds }
          let noSwitchResults = NoSwitch(race), MidRace(0), players, newGameSound
          // The race is over as soon as the last player finishes
          if latestPlacing <> oldLastPlacing then
            // Check if there are any players that are still racing
            match players |> List.tryFind (fun player -> not player.finished) with
            | Some _ -> NoSwitch(race), MidRace(latestPlacing), players, newGameSound
            | None -> NoSwitch(race), initPostRaceState Button.defaultButtonSize settings, players, newGameSound
          else NoSwitch(race), MidRace(latestPlacing), players, newGameSound
        // No player placings
        | PostRace continueButton ->
          let players, _, chariotSounds = nextPlayers nextPlayer 0 playerCollisions gameSound.Chariots race.players
          let raceState = PostRace(Button.next continueButton mouse)
          let raceStatus =
            match continueButton.buttonState with
            | Releasing -> SwitchToAwards
            | _ -> NoSwitch(race)
          raceStatus, raceState, players, { gameSound with Chariots = chariotSounds }
      
      let players = applyPlayerEffects players
      match raceStatus with
      | NoSwitch race -> NoSwitch({ race with raceState = raceState; players = players; timer = race.timer + 1 }), gameSound
      | _ -> raceStatus, gameSound