namespace CircusMaximus.State
open System

type LastPlacing = int

type DynamicRaceState = | MidRace of LastPlacing | PostRace

type RaceState = | PreRace | DynamicRace of DynamicRaceState

type Race = { rand: Random; raceState: RaceState; players: Player list; timer: int }