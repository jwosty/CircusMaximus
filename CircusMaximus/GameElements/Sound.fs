namespace CircusMaximus.State
open System
open System.Diagnostics
open Microsoft.Xna.Framework

/// A descriminated union of all the possible states a sound can be in. Why should only game objects get an
/// abstract, immutable state?
[<DebuggerDisplay("SoundState = {DebugString}")>]
type SoundState =
  /// Represents a sound that is playing n times
  Playing of int
  /// Represents a sound that is looping
  | Looping
  /// Represent a paused sound
  | Paused
  /// Represents an idle sound
  | Stopped
  override this.ToString() =
    match this with
    | Playing times -> sprintf "Playing(times = %i)" times
    | Looping -> "Looping"
    | Paused -> "Paused"
    | Stopped -> "Stopped"
  member private this.DebugString = this.ToString()

/// A record to hold the entire game's sound state
type GameSounds =
  { Chariots: SoundState list
    CrowdCheer: SoundState }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module GameSounds =
  let allStopped numChariots =
    { Chariots = List.init numChariots (fun _ -> Stopped)
      CrowdCheer = Stopped }