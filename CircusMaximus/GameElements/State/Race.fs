namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Input
open CircusMaximus.Types.UnitSymbols

type LastPlacing = int

type RaceState = | PreRace | MidRace of LastPlacing | PostRace of ButtonGroup

type Race(elapsedTime, raceState, players) =
  member this.raceState = raceState
  member this.players = players
  member this.elapsedTime: float<s> = elapsedTime

  interface IGameScreen with
    member this.Next deltaTime rand input = Race.next this deltaTime rand input
  
  static member val next = Unchecked.defaultof<_> with get, set
  
  static member preRaceDuration = 3.<s>
  static member preRaceCountdown = 3.
  static member preRaceDurationPerCount = Race.preRaceDuration / Race.preRaceCountdown
  
  /// The amount of time into the race that it can still be said that it has "just begun"
  static member midRaceBeginPeriod = Race.preRaceDurationPerCount * 2.
  
  /// Finishes players that made the last lap
  static member maxTurns = 4
  
  static member initPostRaceState defaultButtonSize fields =
    PostRace(ButtonGroup.init
      [ Button.initCenter
          (fields.settings.windowDimensions.X / 2.f @@ fields.settings.windowDimensions.Y / 6.f)
          defaultButtonSize "Continue" ])
  
  static member init (playerHorses: _ list) settings =
    let playerY n = (float32 n - 1.f) * 210.f<px> + 740.f<px>
    new Race(
      0.<s>, PreRace,
      [ for n in 1..5 -> Player.init playerHorses.[n - 1] (new PlayerShape(820.f<px> @@ playerY n, PlayerShape.standardDimensions, 0.<r>)) n ])