namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type AwardScreen =
  { timer: int
    /// A list of player data and the amounts they just earned
    playerDataAndWinnings: (PlayerData * int) list
    /// A list of players' horses saved from the last race, which will be passed onto the next
    playerHorses: Horses list
    buttonGroup: ButtonGroup }