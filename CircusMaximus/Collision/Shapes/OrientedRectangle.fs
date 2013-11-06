namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment

type OrientedRectangle =
  struct
    val public Center: Vector2
    val public Width: float32
    val public Height: float32
    val public Direction: float
    
    new(pos, direction, w, h) = { Center = pos; Width = w; Height = h; Direction = direction }
    
    member this.X with get() = this.Center.X
    member this.Y with get() = this.Center.Y
    member this.HalfWidth with get() = this.Width / 2.0f
    member this.HalfHeight with get() = this.Height / 2.0f
    
    /// The corners in clockwise order, starting at the top-left
    member this.Corners =
      let origin, direction = this.Center, this.Direction
      // Offsets from the center
      ( this.HalfWidth  @@ -this.HalfHeight,
        this.HalfWidth  @@ this.HalfHeight,
        -this.HalfWidth @@ this.HalfHeight,
        -this.HalfWidth @@ -this.HalfHeight)
      // Rotate the points around the center by applying a rotation matrix, and ofsetting by the origin
        |> Tuple.t4Map (fun v -> Vector2.Transform(v, Matrix.CreateRotationZ(float32 direction)) + origin)
    
    /// Edges specified as line segments that make up the rectangle, in the form of pairs of points,
    /// starting with the player's left
    member this.Edges = this.Corners |> Tuple.t4ConsecutivePairs
    
    /// Draws the bounding box's boundries
    member this.Draw(sb: SpriteBatch, pixelTexture, redLines) =
      Tuple.t4Iter2
        (fun isRed (start, ``end``) ->
          let color = if isRed then Color.Red else Color.White
          sb.DrawLine(pixelTexture, start, ``end``, color))
        redLines
        this.Edges
  end