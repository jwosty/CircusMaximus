/// Manipulates linear equations in slope-intercept
module CircusMaximus.LinearEquation
open System
open Microsoft.Xna.Framework
open HelperFunctions

/// Stores the slope and intercept of a linear equation
type Line = { m: float32; b: float32 }

/// Solves a linear equation for y
let solveForY (eq: Line) x =
  (eq.m * x) + eq.b

/// Solves a linear equation for x
let solveForX (eq: Line) y =
  (y - eq.b) / eq.m

/// Solves a system of linear equations
let solveSystem (eq1: Line, eq2: Line) =
  // The formula we can get by solving the system for x by hand
  let x = (eq1.b - eq2.b) / (eq2.m - eq1.m)
  // Doesn't matter which equation we use; both will give the same answer. Elementary algebra.
  x @@ solveForY eq1 x

/// Returns a linear equation that both points lie on
let fromPoints (p1: Vector2, p2: Vector2) =
  let m = (p1.Y - p2.Y) / (p1.X - p2.X)
  let b = p1.Y - (solveForY { m = m; b = 0.0f } p1.X)
  { m = m; b = b }