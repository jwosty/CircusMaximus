module CircusMaximus.Collision
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment
open CircusMaximus.Extensions
open CircusMaximus.TupleClassExtensions

type Bounds2D =
  | BoundingLineSegment of LineSegment
  | BoundingRectangle of OrientedRectangle

type CollisionResult =
  | Result_LR of bool
  | Result_BR of bool * bool * bool * bool

/// Combines two collision results into one. Only works for the same type of result.
let combineResultsPair a b =
  match a, b with
    | Result_LR a, Result_LR b -> Result_LR(a || b)
    | Result_BR(a1, a2, a3, a4), Result_BR(b1, b2, b3, b4) -> Result_BR(a1 || b1, a2 || b2, a3 || b3, a4 || b4)
    | _ -> raise (new ArgumentException(sprintf "Only two collision results of the same kind are compatible (got %s and %s)" (a.GetType().Name) (b.GetType().Name)))

/// Combines multiple collision results into one, typically all of the same object
let combineResults results = List.reduce combineResultsPair results

/// Tests if two line segments intersect (maths come from http://stackoverflow.com/a/565282/1231925)
let testIntersection_LineSeg_LineSeg ((p, pr): LineSegment) ((q, qs): LineSegment) =
  let qp = q - p
  let r = pr - p
  let s = qs - q
  
  let qmpxr = cross qp r
  let qmpxs = cross qp s
  let rxs = cross r s
  
  if qmpxr = 0.0f then
    (((q.X - p.X < 0.0f) <> (q.X - pr.X < 0.0f))
      || ((q.Y - p.Y < 0.0f) <> (q.Y - pr.Y < 0.0f))) |> twice
  else
  if rxs = 0.0f then
    false, false
  else
    let rxsr = 1.0f / rxs
    let t = qmpxs * rxsr
    let u = qmpxr * rxsr
    
    (t >=< (0.0f, 1.0f) && u >=< (0.0f, 1.0f)) |> twice

let collide_LineSeg_LineSeg a b =
  testIntersection_LineSeg_LineSeg a b |> Tuple.t2Map Result_LR

/// Returns the indices of every edge that is intersecting the given line segment
let collide_ORect_LineSeg (rect: OrientedRectangle) (seg: LineSegment) =
  let resultR, resultS =
    Tuple.t4Map (fun edge -> testIntersection_LineSeg_LineSeg edge seg) rect.Edges
      |> Tuple.t4Unzip2
  Result_BR(resultR), Result_LR(Tuple.t4Reduce (||) resultS)

/// Returns a tuple containing the intersecting lines of each bounding box
let collide_ORect_ORect (a: OrientedRectangle) (b: OrientedRectangle) =
  Result_BR(
    a.Edges
      |> Tuple.t4Map
          (fun aEdge ->
            b.Edges
              |> Tuple.t4Map (fun bEdge -> testIntersection_LineSeg_LineSeg aEdge bEdge |> fst)
              |> Tuple.t4Reduce (||))),
  Result_BR(
    b.Edges
      |> Tuple.t4Map
          (fun bEdge ->
            a.Edges
              |> Tuple.t4Map (fun aEdge -> testIntersection_LineSeg_LineSeg bEdge aEdge |> fst)
              |> Tuple.t4Reduce (||)))

let collidePair a b =
  match a, b with
    | BoundingLineSegment(a), BoundingLineSegment(b) -> collide_LineSeg_LineSeg a b
    | BoundingRectangle(a), BoundingRectangle(b) -> collide_ORect_ORect a b
    | BoundingRectangle(a), BoundingLineSegment(b) -> collide_ORect_LineSeg a b
    | BoundingLineSegment(a), BoundingRectangle(b) -> collide_ORect_LineSeg b a

let nextIndex length i =
  //let iLen = length objects
  if i <= (length - 2) then
    i + 1
  else
    0

/// Calculates the intersections of a list of items. Not optimized at all yet
let collideWorld objects =
  objects
    // Collide all the objects together
    |> List.mapi (fun i obj -> objects |> List.removeIndex i |> List.map (fun othObj -> collidePair obj othObj |> fst))
    // Combine the results for each object together
    |> List.map (fun objResults -> combineResults objResults)