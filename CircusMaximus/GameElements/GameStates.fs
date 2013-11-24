namespace CircusMaximus.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision
open CircusMaximus.Input
open CircusMaximus.Player

type CommonRaceData =
  struct
    val players: Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

/// Game state machine
type GameState =
  | PreRace of CommonRaceData
  /// int parameter is the index of the player most recently finished
  | MidRace of CommonRaceData * int
  | PostRace of CommonRaceData

type Game = GameState