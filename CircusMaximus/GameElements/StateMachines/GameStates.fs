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

type RaceState = | PreRace | MidRace of LastPlacing | PostRace

type Race = { raceState: RaceState; players: Player list; timer: int }