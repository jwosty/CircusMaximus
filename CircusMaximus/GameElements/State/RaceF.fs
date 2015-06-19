namespace CircusMaximus.Functions
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

module Race =
  /// Calculates the intersections for all objects
  let collideWorld players racetrackBounds = racetrackBounds :: (List.map Player.getBB players) |> Collision.collideWorld
  
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
          let player, lastPlacing = Player.nextPlayerFinish Race.maxTurns lastPlacing player
          nextPlayers
            (updatedPlayers @ [player]) lastPlacing (updatedChariotSounds @ [nextPlayerChariotSound])
            nextPlayerFunction restPlayerCollisions restPlayerChariotSounds restPlayers
      | [], [], [] -> updatedPlayers, lastPlacing, updatedChariotSounds
      | _ -> raise (new ArgumentException("The lists had different lengths."))
    
    let players, latestPlacing, soundState = nextPlayers [] latestPlacing [] playerMapper playerCollisions playerChariotSounds players
    players |> applyPlayerEffects, latestPlacing, soundState
  
  /// A quick 'n dirty function to find the last player placing so we know what to use next when someone
  /// else finishes.
  let getLastPlacing players =
    List.fold
      (fun lastPlacing (player: Player) -> if player.finished then lastPlacing + 1 else lastPlacing)
      0 players

  let switchToAwardScreen (race: Race) fields =
    let playerDataAndWinnings =
      race.players |> List.map
        (fun player ->
          let winnings =
            match player.finishState with
            | Finished placing -> PlayerData.playerWinnings placing
            // Something strange is happening if there's an unfinished player after the race has ended
            | Racing -> 0
          PlayerData.findByNumber player.number fields.playerData, winnings)
    let playerHorses = race.players |> List.map (fun player -> player.horses)
    let awardScreen, playerData = AwardScreen.init fields playerDataAndWinnings playerHorses
    awardScreen :> IGameScreen,
    { fields with playerData = playerData }
  
  /// Returns the next race state. 
  let next (race: Race) deltaTime fields input =
    let nextPlayer = Player.next fields input Racetrack.collisionShape.RespawnPath
    match race.raceState with
    | PreRace ->
      if race.elapsedTime >= Race.preRaceDuration then
        Some(
          // Begin the race when it's time
          new Race(
            0.<s>, MidRace(getLastPlacing race.players),  // In case we needed to hard-code some players to start into the pre-race state; this should normally return 0
            race.players) :> IGameScreen,
          { fields with sounds = { fields.sounds with CrowdCheer = Playing 1 } } )   // The crowd gets exited when the race begins
      else
        Some(
          upcast new Race(race.elapsedTime + deltaTime, race.raceState, race.players),   // Simply increment the timer until the race starts
          fields)   // No sounds here
    
    | _ ->
      let playerCollisions = collideWorld race.players Racetrack.collisionBounds |> List.tail
      // "Dynamic" races require player updates, but player placings are only used before the race is over. These two conditions
      // mean that Player.next can't do placings, so we have to do it down there somewhere.
      let (screen: IGameScreen), sounds =
        match race.raceState with
        | MidRace oldLastPlacing ->
          let players, latestPlacing, playerChariotSounds = nextPlayers nextPlayer oldLastPlacing playerCollisions fields.sounds.Chariots race.players
          let sounds =
            { CrowdCheer =
                if oldLastPlacing <> latestPlacing   // Congratulate the player for finishing in the top 3
                then Playing 1
                else fields.sounds.CrowdCheer
              Chariots = playerChariotSounds }
          // The race is over as soon as the last player finishes
          let raceState =
            if latestPlacing <> oldLastPlacing then
              // Check if there are any players that are still racing
              match players |> List.tryFind (fun player -> not player.finished) with
                | Some _ -> MidRace(latestPlacing)
                | None -> Race.initPostRaceState Button.defaultButtonDimensions fields
            else MidRace(latestPlacing)
          upcast new Race(race.elapsedTime + deltaTime, raceState, players), sounds
        // No player placings
        | PostRace buttonGroup ->
          let players, _, chariotSounds = nextPlayers nextPlayer 0 playerCollisions fields.sounds.Chariots race.players
          let fields = { fields with sounds = { fields.sounds with Chariots = chariotSounds } }
          let race =
            match buttonGroup.buttons.[0].buttonState with
            | Releasing -> fst <| switchToAwardScreen race fields
            | _ -> upcast race
          race, { fields.sounds with Chariots = chariotSounds }
      
      Some(screen, { fields with sounds = sounds })