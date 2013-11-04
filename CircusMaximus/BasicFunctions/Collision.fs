module CircusMaximus.Collision
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment

type Bounds2D =
  | BoundingLineSegment of LineSegment
  | BoundingRectangle of OrientedRectangle

type CollisionResults =
  | Result_LS of bool
  | Result_BR of bool list

/// Tests if two line segments intersect (maths come from http://stackoverflow.com/a/565282/1231925)
let collide_LineSeg_LineSeg ((p, pr): LineSegment) ((q, qs): LineSegment) =
  let qp = q - p
  let r = pr - p
  let s = qs - q
  
  let qmpxr = cross qp r
  let qmpxs = cross qp s
  let rxs = cross r s
  
  if qmpxr = 0.0f then
       ((q.X - p.X < 0.0f) <> (q.X - pr.X < 0.0f))
    || ((q.Y - p.Y < 0.0f) <> (q.Y - pr.Y < 0.0f))
  else
    if rxs = 0.0f then
      false
    else
      let rxsr = 1.0f / rxs
      let t = qmpxs * rxsr
      let u = qmpxr * rxsr
      
      t >=< (0.0f, 1.0f) && u >=< (0.0f, 1.0f)

/// Returns the indices of every edge that is intersecting the given line segment
let collide_ORect_LineSeg (rect: OrientedRectangle) (seg: LineSegment) =
  List.map (fun edge -> collide_LineSeg_LineSeg edge seg) rect.Edges

/// Returns the indices of every edge that is intersecting a bounding box
let collide_ORect_ORect (a: OrientedRectangle) (b: OrientedRectangle) =
  List.map (fun edge -> edge |> (collide_ORect_LineSeg b) |> List.exists id) a.Edges

let collide = function
  | BoundingLineSegment a, BoundingLineSegment b -> Result_LS(collide_LineSeg_LineSeg a b)
  | BoundingRectangle a, BoundingRectangle b -> Result_BR(collide_ORect_ORect a b)
  | BoundingRectangle a, BoundingLineSegment b -> Result_BR(collide_ORect_LineSeg a b)
  | BoundingLineSegment a, BoundingRectangle b -> Result_BR(collide_ORect_LineSeg b a)