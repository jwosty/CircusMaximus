module CircusMaximus.BoundingBox2D
open System
open Microsoft.Xna.Framework
open HelperFunctions
open LineSegment

// Position, width, height
type BoundingBox2D =
  struct
    val public Center: Vector2
    val public Width: float32
    val public Height: float32
    
    new(pos, w, h) = { Center = pos; Width = w; Height = h }
    
    member this.X with get() = this.Center.X
    member this.Y with get() = this.Center.Y
    
    /// The corners in clockwise order
    member this.Corners =
      [ this.Center;
        this.Center + (this.Width @@ 0);
        this.Center + (this.Width @@ this.Height)
        this.Center + (0 @@ this.Height); ]
    
    /// Edges specified as line segments that make up the rectangle, in the form of pairs of points
    member this.Edges = this.Corners |> List.consecutivePairs
    
    /// Tests if this bounding box collides with a line segment
    member this.Intersects lineSegment =
      match (this.Edges |> List.tryFind (fun edge -> edge -+- lineSegment)) with
        | Some _ -> true
        | None -> false
    
    // Tests if this bounding box intersects the other bounding box
    member this.Intersects (boundingBox: BoundingBox2D) =
      match (this.Edges |> List.tryFind (fun edge -> boundingBox.Intersects edge)) with
        | Some _ -> true
        | None -> false
  end