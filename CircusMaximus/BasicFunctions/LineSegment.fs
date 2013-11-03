module CircusMaximus.LineSegment
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions

type LineSegment = Vector2 * Vector2

/// Tests whether or not a point is on a line segment
let (-*-) ((lp1, lp2): LineSegment) (p: Vector2) = p.X >=< (lp1.X, lp2.X)

/// Tests whether or not the line segments intersect
(*
let (-+-) a b =
  let aEq, bEq = LinearEquation.fromPoints a, LinearEquation.fromPoints b
  if aEq.m = bEq.m && aEq.b <> bEq.b then
    // a and b are parallel but have a different y intercept
    false
  else
    let intersection = LinearEquation.solveSystem (aEq, bEq)
    a -*- intersection || b -*- intersection
*)
//let (-+-) a b = false

/// Rotates both line segments uniformly so that A is vertical, and return B
let rotateLineSegments ((a1, a2): LineSegment) ((b1, b2): LineSegment) : LineSegment =
  let origin = Vector2.Lerp(a1, a2, 0.5f)
  let rotationMatrix =
    let p = a2 - a1
    Matrix.CreateRotationZ(atan2 p.X p.Y)
  let rotate x = Vector2.Transform(x - origin, rotationMatrix) + origin
  rotate b1, rotate b2

let (-+-) a b =
  // Two line segments are intersecting if their points are on opposite sides of each other
  // This can be determined by rotating them both so that one is vertical, and testing the X values,
  // then repeating the same process, reversing the roles
  let rotB1, rotB2 = rotateLineSegments a b
  if 0.0f >=< (rotB1.X, rotB2.X) || 0.0f >=< (rotB1.Y, rotB2.Y) then
    true
    //let rotA1, rotA2 = rotateLineSegments b a
    //0.0f >=< (rotA1.X, rotA2.X) || 0.0f >=< (rotA1.Y, rotA2.Y)
  else
    false