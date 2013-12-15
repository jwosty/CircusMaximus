namespace CircusMaximus.State
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
/// A single particle effect where the position is defined to be relative to a parent object
type BoundParticle =
  { position: Vector2; direction: float; age: float }
  static member RandInit (rand: Random) = { position = 0 @@ 0; direction = rand.NextDouble() * (float MathHelper.TwoPi); age = 0.}
  static member RandBatchInit (rand: Random) =
    List.init
      ((rand.NextDouble() ** 32.) * 3. |> round |> int)
      (fun _ -> BoundParticle.RandInit rand)