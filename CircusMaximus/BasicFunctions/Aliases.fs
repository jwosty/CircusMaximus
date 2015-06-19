namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Core.LanguagePrimitives
open Microsoft.Xna.Framework
open CircusMaximus

/// A velocity, in percentage (where 0 = 0% and 1 = 100%) of a top speed
type Velocity = float
/// A player's finishing place in the race
type Placing = int
/// A length of time
type Duration = int
/// A taunt contains the string to display as well as the amount of time it lasts for
type Taunt = string * Duration

type Vector2<[<Measure>] 'u>(xnaVector2: Vector2) =
  member this.xnaVector2 = xnaVector2
  new(x: float32<'u>, y: float32<'u>) = new Vector2<_>(new Vector2(float32 x, float32 y))
  member this.X = Float32WithMeasure<'u> xnaVector2.X
  member this.Y = Float32WithMeasure<'u> xnaVector2.Y
  member this.LengthSquared = (this.X * this.X) + (this.Y * this.Y)
  member this.Length = sqrt this.LengthSquared
  member this.Normalized = new Vector2<1>(this.X / this.Length, this.Y / this.Length)
  
  static member Zero = new Vector2<'u>(Vector2.Zero)
  static member One = new Vector2<'u>(Vector2.One)
  
  static member ( + ) (v1: Vector2<_>, v2: Vector2<_>) = new Vector2<_>(v1.X + v2.X, v1.Y + v2.Y)
  static member ( - ) (v1: Vector2<_>, v2: Vector2<_>) = new Vector2<_>(v1.X - v2.X, v1.Y - v2.Y)
  static member ( * ) (v1: Vector2<_>, v2: Vector2<_>) = new Vector2<_>(v1.X * v2.X, v1.Y * v2.Y)
  static member ( * ) (vector: Vector2<_>, scalar: float32<_>) = new Vector2<_>(vector.X * scalar, vector.Y * scalar)
  static member ( * ) (vector: Vector2<'u>, scalar: float<'v>) = vector * (scalar |> float32 |> Float32WithMeasure<'v>)
  static member ( * ) (vector: Vector2<'u>, scalar: int<'v>) = vector * (scalar |> float32 |> Float32WithMeasure<'v>)
  static member ( / ) (v1: Vector2<_>, v2: Vector2<_>) = new Vector2<_>(v1.X / v2.X, v1.Y / v2.Y)
  static member ( / ) (vector: Vector2<_>, scalar: float32<_>) = new Vector2<_>(vector.X / scalar, vector.Y / scalar)
  static member ( / ) (vector: Vector2<'u>, scalar: float<'v>) = vector / (scalar |> float32 |> Float32WithMeasure<'v>)
  static member ( / ) (vector: Vector2<'u>, scalar: int<'v>) = vector / (scalar |> float32 |> Float32WithMeasure<'v>)
  
  static member Distance (vector1: Vector2<_>, vector2: Vector2<_>) = (vector1 - vector2).Length
  static member Transform (vector: Vector2<'u>, matrix: Matrix) = new Vector2<'u>(Vector2.Transform (vector.xnaVector2, matrix))