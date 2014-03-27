namespace CircusMaximus.Input
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.Types
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

type PlayerInput =
  { /// How hard the player is pulling on the left reign; ranges from 0 to 1
    leftReignPull: float
    /// How hard the player is puling on the right reign; ranges from 0 to 1
    rightReignPull: float
    /// Whether or not the player is pressing the button to flick the reigns (controls speed)
    flickReigns: bool
    /// True when the player presses the taunt button (Q on the keyboard, A on the controller)
    expectingTaunt: bool
    /// Debug input to advance one lap
    advanceLap: bool
    /// Indicates how many items to move the item selector (negative = left, positive = right, 0 = none)
    selectorΔ: int
    /// Whether or not the user pressed the use item button
    isUsingItem: bool }