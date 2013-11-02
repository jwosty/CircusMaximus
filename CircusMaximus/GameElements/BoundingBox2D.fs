module CircusMaximus.BoundingBox2D
open System
open Microsoft.Xna.Framework
open HelperFunctions

// For now, it's an AABB
type BoundingBox2D = { Position: Vector2; Width: float32; Height: float32 }

// The corners of a bounding box
let corners (bb: BoundingBox2D) =
  [ bb.Position + (       0 @@ 0        );
    bb.Position + (bb.Width @@ 0        );
    bb.Position + (       0 @@ bb.Height);
    bb.Position + (bb.Width @@ bb.Height) ]

// The edges of a bounding box
let edges bb = corners bb |> List.consecutivePairs