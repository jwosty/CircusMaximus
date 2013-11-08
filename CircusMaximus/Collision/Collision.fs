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
  | BoundingPolygon of Polygon

let drawUniformBounds pixelTexture (sb: SpriteBatch) color = function
  | BoundingLineSegment seg -> drawLineSegment pixelTexture sb color seg
  | BoundingPolygon poly -> poly.Draw(sb, pixelTexture, List.init (poly.Edges.Length) (fun _ -> false))

type CollisionResult =
  | Result_Line of bool
  | Result_Poly of bool list

/// Combines two collision results into one. Only works for the same type of result.
let combineResultsPair a b =
  match a, b with
    | Result_Line a, Result_Line b -> Result_Line(a || b)
    | Result_Poly a, Result_Poly b -> Result_Poly(List.map2 (||) a b)
    | _ -> raise (new ArgumentException(sprintf "Only two collision results of the same kind are compatible (got %s and %s)" (a.GetType().Name) (b.GetType().Name)))

/// Combines multiple collision results into one, typically all of the same object
let combineResults results = List.reduce combineResultsPair results

let collide_LineSeg_LineSeg a b = a -+- b |> twice |> Tuple.t2Map Result_Line

/// Returns the indices of every edge that is intersecting the given line segment
let collide_Poly_LineSeg (poly: Polygon) (seg: LineSegment) =
  let resultP, resultS =
    List.map (fun edge -> edge -+- seg |> twice) poly.Edges
      |> List.unzip
  Result_Poly(resultP), Result_Line(List.reduce (||) [true; true; false])

/// Returns a tuple containing the intersecting lines of each bounding box
let collide_Poly_Poly (a: Polygon) (b: Polygon) =
  Result_Poly(
    a.Edges
      |> List.map
          (fun aEdge ->
            b.Edges
              |> List.map ((-+-) aEdge)
              |> List.reduce (||))),
  Result_Poly(
    b.Edges
      |> List.map
          (fun bEdge ->
            a.Edges
              |> List.map ((-+-) bEdge)
              |> List.reduce (||)))

let collidePair a b =
  match a, b with
    | BoundingLineSegment(a), BoundingLineSegment(b) -> collide_LineSeg_LineSeg a b
    | BoundingPolygon(a), BoundingPolygon(b) -> collide_Poly_Poly a b
    | BoundingPolygon(a), BoundingLineSegment(b) -> collide_Poly_LineSeg a b
    | BoundingLineSegment(a), BoundingPolygon(b) -> collide_Poly_LineSeg b a

/// Calculates the intersections of a list of items. Not optimized at all yet
let collideWorld objects =
  // Imperitive optimized code
  let results = Array.create (List.length objects) List.empty
  for iA in 0..(objects.Length - 1) do
    for iB in 0..(objects.Length - 1) do
      if iA < iB then
        let rA, rB = collidePair objects.[iA] objects.[iB]
        results.[iA] <- rA :: results.[iA]
        results.[iB] <- rB :: results.[iB]
  results
    |> List.ofArray
    |> List.map (fun objResults -> combineResults objResults)