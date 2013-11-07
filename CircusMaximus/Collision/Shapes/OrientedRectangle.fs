namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment

type OrientedRectangle(center, width, height, direction) =
  inherit Polygon(center)
  
  member this.Width: float32 = width
  member this.Height: float32 = height
  member this.Direction: float = direction
  
  member this.X with get() = this.Center.X
  member this.Y with get() = this.Center.Y
  member this.HalfWidth with get() = this.Width / 2.0f
  member this.HalfHeight with get() = this.Height / 2.0f
  
  /// The corners in clockwise order, starting at the top-left
  override this.Points =
    let origin, direction = this.Center, this.Direction
    // Offsets from the center
    [ this.HalfWidth  @@ -this.HalfHeight;
      this.HalfWidth  @@ this.HalfHeight;
      -this.HalfWidth @@ this.HalfHeight;
      -this.HalfWidth @@ -this.HalfHeight]
    // Rotate the points around the center by applying a rotation matrix, and ofsetting by the origin
      |> List.map (fun v -> Vector2.Transform(v, Matrix.CreateRotationZ(float32 direction)) + origin)