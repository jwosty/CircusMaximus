module CircusMaximus.BoundingBox2D
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment

// Position, width, height
type BoundingBox2D =
  struct
    val public Center: Vector2
    val public Width: float32
    val public Height: float32
    
    new(pos, w, h) = { Center = pos; Width = w; Height = h }
    
    member this.X with get() = this.Center.X
    member this.Y with get() = this.Center.Y
    member this.HalfWidth with get() = this.Width / 2.0f
    member this.HalfHeight with get() = this.Height / 2.0f
    
    /// The corners in clockwise order
    member this.Corners =
      [ this.Center + (-this.HalfWidth @@ -this.HalfHeight);
        this.Center + ( this.HalfWidth @@ -this.HalfHeight);
        this.Center + ( this.HalfWidth @@ this.HalfHeight );
        this.Center + (-this.HalfWidth @@ this.HalfHeight )]
    
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
    
    /// Draws the bounding box's boundries
    member this.Draw(sb: SpriteBatch, pixelTexture) = this.Edges |> List.iter (fun (start, ``end``) -> sb.DrawLine(pixelTexture, start, ``end``))
  end