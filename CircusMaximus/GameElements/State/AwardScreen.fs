namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Input

type AwardScreen = { timer: int }

/// Contains functions and constants pertaining to award screens
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AwardScreen =
  /// The default initialized award screen
  let initted = { timer = 0 }
  
  /// Updates an award screen and returns the new model
  let next (awardScreen: AwardScreen) = NoSwitch({ timer = awardScreen.timer + 1 })