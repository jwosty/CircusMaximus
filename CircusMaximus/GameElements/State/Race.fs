namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type LastPlacing = int

type RaceState = | PreRace | MidRace of LastPlacing | PostRace of ButtonGroup

type Race =
  { raceState: RaceState; players: Player list; timer: int }

  static member preRaceTicks = 200
  static member preRaceMaxCount = 3
  static member preRaceTicksPerCount = float Race.preRaceTicks / float Race.preRaceMaxCount |> ceil |> int
  
  /// The amount of time into the race that it can still be said that it has "just begun"
  static member midRaceBeginPeriod = Race.preRaceTicksPerCount * 2
  
  /// Finishes players that made the last lap
  static member maxTurns = 13
  
  static member initPostRaceState defaultButtonSize (settings: GameSettings) =
    PostRace(ButtonGroup.init
      [ Button.initCenter
          (settings.windowDimensions.X / 2.f @@ settings.windowDimensions.Y / 6.f * 1.f)
          defaultButtonSize "Continue" ])
  
  static member init (playerHorses: _ list) settings =
    let playerY n = (n - 1) * 210 + 740 |> float32
    { raceState = PreRace
      players =
        [ for n in 1..5 -> Player.init playerHorses.[n - 1] (new PlayerShape(820.f @@ playerY n, 64.0f, 29.0f, 0.)) n ]
      timer = 0 }