namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type LastPlacing = int

type RaceState = | PreRace | MidRace of LastPlacing | PostRace of Button * Button

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
    let initButton y label = Button.initCenter (settings.windowDimensions.X / 2.f @@ settings.windowDimensions.Y * 0.1f * float32 y) defaultButtonSize label
    PostRace(initButton 1 "Continue", initButton 2 "Exit races")
  
  let init settings =
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
        { player with finishState = Finished(lastPlacing + 1) }, lastPlacing + 1
      else
        player, lastPlacing
    | Finished _ -> player, lastPlacing
  
  let nextPlayer (lastKeyboard: KeyboardState, keyboard) (lastGamepads: GamePadState list, gamepads: _ list) rand collisionResult playerChariotSound player =
    let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen!"
    let player, playerChariotSound =
      if player.number = 1 then
        Player.next
          (PlayerInput.initFromKeyboard (lastKeyboard, keyboard) PlayerInput.maxTurn PlayerInput.maxSpeed)
          player collision Racetrack.center rand playerChariotSound
      else
        let lastGamepad, gamepad = lastGamepads.[player.number - 2], gamepads.[player.number - 2]
        Player.next
          (PlayerInput.initFromGamepad (lastGamepad, gamepad) PlayerInput.maxTurn PlayerInput.maxSpeed)
          player collision Racetrack.center rand playerChariotSound
    player, playerChariotSound
  
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
          let player, lastPlacing = nextPlayerFinish lastPlacing player
          nextPlayers
            (updatedPlayers @ [player]) lastPlacing (updatedChariotSounds @ [nextPlayerChariotSound])
            nextPlayerFunction restPlayerCollisions restPlayerChariotSounds restPlayers
      | [], [], [] -> updatedPlayers, lastPlacing, updatedChariotSounds
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    // Not curried because it gives an ugly function signature
    nextPlayers [] latestPlacing [] playerMapper playerCollisions playerChariotSounds players
  
  /// Returns the next race state. 
  let next (race: Race) mouse (lastKeyboard, keyboard) (lastGamepads, gamepads) rand gameSound (settings: GameSettings) =
    let nextPlayer = nextPlayer (lastKeyboard, keyboard) (lastGamepads, gamepads) rand
    match race.raceState with
    | PreRace ->
      let nextRace, gameSounds =
        if race.timer >= preRaceTicks then
          { raceState = MidRace(0); players = race.players; timer = 0 },  // Begin the race when it's time
          { gameSound with CrowdCheer = Playing 1 }   // The crowd gets exited when the race begins
        else
          { race with timer = race.timer + 1 },   // Simply increment the timer until the race starts
          gameSound   // No sounds here
      Some(nextRace), gameSounds
    
    | _ ->
      let playerCollisions = collideWorld race.players Racetrack.collisionBounds |> List.tail
      // "Dynamic" races require player updates, but player placings are only used before the race is over. These two conditions
      // mean that Player.next can't do placings, so we have to do it down there somewhere.
      let raceState, players, gameSound, shouldExitRaces =
        match race.raceState with
        | MidRace oldLastPlacing ->
          let players, latestPlacing, playerChariotSounds =
            nextPlayers nextPlayer oldLastPlacing playerCollisions gameSound.Chariots race.players
          let newGameSound =
            { CrowdCheer =
                if oldLastPlacing <> latestPlacing   // Congratulate the player for finishing in the top 3
                then Playing 1
                else gameSound.CrowdCheer
              Chariots = playerChariotSounds }
          // The race is over as soon as the last player finishes
          if latestPlacing = players.Length
          then initPostRaceState Button.defaultButtonSize settings, players, newGameSound, false
          else MidRace(0), players, newGameSound, false
        // No player placings
        | PostRace(continueButton, exitButton) ->
          let players, _, chariotSounds = nextPlayers nextPlayer 0 playerCollisions gameSound.Chariots race.players
          let raceState = PostRace(Button.next continueButton mouse, Button.next exitButton mouse)
          let shouldExitRaces =
            match exitButton.buttonState with
            | Releasing -> true
            | _ -> false
          raceState, players, { gameSound with Chariots = chariotSounds }, shouldExitRaces
      
      let players = applyPlayerEffects players
      if shouldExitRaces
        then None, gameSound
        else Some({ raceState = raceState; players = players; timer = race.timer + 1 }), gameSound