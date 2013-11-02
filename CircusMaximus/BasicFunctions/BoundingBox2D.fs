module CircusMaximus.BoundingCircle
open System
open Microsoft.Xna.Framework
open HelperFunctions

// Position, width, height
type BoundingBox2D =
  struct
    val public Position: Vector2
    val public Width: float32
    val public Height: float32
    member this.X with get() = this.Position.X
    member this.Y with get() = this.Position.Y
    
    /// The corners in clockwise order
    member this.Corners =
      [ this.Position;
        this.Position + (this.Width @@ 0);
        this.Position + (0 @@ this.Height);
        this.Position + (this.Width @@ this.Height) ]
    
    /// Edges specified as line segments that make up the rectangle, in the form of pairs of points
    member this.Edges = this.Corners |> List.consecutivePairs
  end