module CircusMaximus.Collision
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.LineSegment
open CircusMaximus.Extensions
open CircusMaximus.TupleClassExtensions

type Bounds2D =
  | BoundingLineSegment of LineSegment
  | BoundingRectangle of OrientedRectangle

let drawUniformBounds pixelTexture (sb: SpriteBatch) color = function
  | BoundingLineSegment seg -> drawLineSegment pixelTexture sb color seg
  | BoundingRectangle rect -> rect.Draw(sb, pixelTexture, (false, false, false, false))

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

let collide_LineSeg_LineSeg a b = a -+- b |> twice |> Tuple.t2Map Result_LR

/// Returns the indices of every edge that is intersecting the given line segment
let collide_ORect_LineSeg (rect: OrientedRectangle) (seg: LineSegment) =
  let resultR, resultS =
    Tuple.t4Map (fun edge -> edge -+- seg |> twice) rect.Edges
      |> Tuple.t4Unzip2
  Result_BR(resultR), Result_LR(Tuple.t4Reduce (||) resultS)

/// Returns a tuple containing the intersecting lines of each bounding box
let collide_ORect_ORect (a: OrientedRectangle) (b: OrientedRectangle) =
  Result_BR(
    a.Edges
      |> Tuple.t4Map
          (fun aEdge ->
            b.Edges
              |> Tuple.t4Map ((-+-) aEdge)
              |> Tuple.t4Reduce (||))),
  Result_BR(
    b.Edges
      |> Tuple.t4Map
          (fun bEdge ->
            a.Edges
              |> Tuple.t4Map ((-+-) bEdge)
              |> Tuple.t4Reduce (||)))

let collidePair a b =
  match a, b with
    | BoundingLineSegment(a), BoundingLineSegment(b) -> collide_LineSeg_LineSeg a b
    | BoundingRectangle(a), BoundingRectangle(b) -> collide_ORect_ORect a b
    | BoundingRectangle(a), BoundingLineSegment(b) -> collide_ORect_LineSeg a b
    | BoundingLineSegment(a), BoundingRectangle(b) -> collide_ORect_LineSeg b a

/// Calculates the intersections of a list of items. Not optimized at all yet
let collideWorld objects =
  objects
    // Collide all the objects together
    |> List.mapi (fun i obj -> objects |> List.removeIndex i |> List.map (fun othObj -> collidePair obj othObj |> fst))
    // Combine the results for each object together
    |> List.map (fun objResults -> combineResults objResults)