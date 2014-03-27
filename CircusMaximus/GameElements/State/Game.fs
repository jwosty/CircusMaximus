namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions

type GameState =
  | GameMainMenu of MainMenu
  | GameTutorial of Tutorial
  | GameHorseScreen of Horses list * ButtonGroup
  | GameRace of Race
  | GameAwardScreen of AwardScreen

/// Holds the state of the entire game
type Game =
    { gameState: GameState
      settings: GameSettings
      rand: Random
      playerData: PlayerData list
      gameSounds: GameSounds }