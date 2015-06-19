namespace CircusMaximus
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

type PlayerShape(center, dimensions, direction) =
  inherit Polygon(center)
  
  static member standardDimensions = 64.f<px> @@ 29.f<px>
  
  member this.Dimensions: Vector2<px> = dimensions
  member this.Direction: float<r> = direction
  
  member this.X with get() = center.X
  member this.Y with get() = center.Y
  member this.HalfDimensions with get() = this.Dimensions / 2.f<_>
  
  /// The corners in clockwise order, starting at the top-left
  override this.Points =
    let origin, direction = this.Center, this.Direction
    // Offsets from the center
    [  this.HalfDimensions.X @@ -this.HalfDimensions.Y;
       this.HalfDimensions.X @@  this.HalfDimensions.Y;
      -this.HalfDimensions.Y @@  this.HalfDimensions.Y;
      -this.HalfDimensions.X @@ -this.HalfDimensions.Y]
    // Rotate the points around the center by applying a rotation matrix, and ofsetting by the origin
      |> List.map (fun v -> Vector2<_>.Transform(v, Matrix.CreateRotationZ(float32 direction)) + origin)