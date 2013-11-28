namespace CircusMaximus.State

type LastPlacing = int

type DynamicRaceState = | MidRace of LastPlacing | PostRace

type RaceState = | PreRace | DynamicRace of DynamicRaceState

type Race = { raceState: RaceState; players: Player list; timer: int }