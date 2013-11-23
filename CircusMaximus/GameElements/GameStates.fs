namespace CircusMaximus.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision
open CircusMaximus.PlayerInput
open CircusMaximus.Player

type PreRaceData =
  struct
    val players: Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

type MidRaceData =
  struct
    val players: Player list
    val timer: int
    val lastPlacing: int
    
    new(players, timer, lastPlacing) = { players = players; timer = timer; lastPlacing = lastPlacing }
  end

type PostRaceData =
  struct
    val players: Player list
    val timer: int
    
    new(players, timer) = { players = players; timer = timer }
  end

/// Game state machine
type GameState =
  | PreRace of PreRaceData
  | MidRace of MidRaceData
  | PostRace of PostRaceData

type Game = GameState