namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

/// A single particle effect where the position is defined to be relative to a parent object
type BoundParticle =
  { position: Vector2<px>; direction: float<r>; age: float<fr>; factor: float; color: Color }
  static member RandInit (rand: Random) factor color = { position = Vector2<_>.Zero; direction = rand.NextDouble() * (float MathHelper.TwoPi) * 1.<r>; age = 0.<fr>; factor = factor; color = color }
  static member RandBatchInit (rand: Random) factor greenProbability =
    List.init
      ((rand.NextDouble() ** 32.) * 3. |> int)
      (fun _ -> BoundParticle.RandInit rand factor (if rand.NextDouble () < greenProbability then Color.Green else Color.White))
  
  static member particleAge = 100.53<fr>