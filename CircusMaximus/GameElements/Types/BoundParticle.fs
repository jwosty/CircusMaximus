namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions

/// A single particle effect where the position is defined to be relative to a parent object
type BoundParticle =
  { position: Vector2; direction: float; age: float; factor: float }
  static member RandInit (rand: Random) factor = { position = 0 @@ 0; direction = rand.NextDouble() * (float MathHelper.TwoPi); age = 0.; factor = factor}
  static member RandBatchInit (rand: Random) factor =
    List.init
      ((rand.NextDouble() ** 32.) * 3. |> round |> int)
      (fun _ -> BoundParticle.RandInit rand factor)