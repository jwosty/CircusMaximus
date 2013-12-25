namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework

/// A descriminated union of all the possible states a sound can be in. Why should only game objects get an
/// abstract, immutable state?
type SoundState =
  Playing of int | Paused | Stopped
  override this.ToString() =
    match this with
    | Playing times -> sprintf "Playing(times = %i)" times
    | Paused -> "Paused"
    | Stopped -> "Stopped"

/// A record to hold the entire game's sound state
type GameSounds =
  { Chariots: SoundState list
    CrowdCheer: SoundState }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module GameSounds =
  let allStopped numChariots =
    { Chariots = List.init numChariots (fun _ -> Stopped)
      CrowdCheer = Stopped }