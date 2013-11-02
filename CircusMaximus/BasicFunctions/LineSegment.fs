module CircusMaximus.LineSegment
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions

type LineSegment = Vector2 * Vector2

/// Tests whether or not a point is on a line segment
let (-*-) ((lp1, lp2): LineSegment) (p: Vector2) = p.X >=< (lp1.X, lp2.X)

/// Tests whether or not the line segments intersect
let (-+-) a b =
  let aEq, bEq = LinearEquation.fromPoints a, LinearEquation.fromPoints b
  if aEq.m = bEq.m && aEq.b <> bEq.b then
    // a and b are parallel but have a different y intercept
    false
  else
    let intersection = LinearEquation.solveSystem (aEq, bEq)
    a -*- intersection || b -*- intersection