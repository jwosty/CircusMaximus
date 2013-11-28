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

type LastPlacing = int

type DynamicRaceState = | MidRace of LastPlacing | PostRace

type RaceState = | PreRace | DynamicRace of DynamicRaceState

type Race = { raceState: RaceState; players: Player list; timer: int }