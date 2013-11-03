module CircusMaximus.LineSegment
open System
open Microsoft.Xna.Framework
open CircusMaximus
open CircusMaximus.HelperFunctions

type LineSegment = Vector2 * Vector2

/// Tests if two line segments intersect (maths come from http://stackoverflow.com/a/565282/1231925)
let (-+-) ((A, B): LineSegment) ((C, D): LineSegment) =
  let CmP = C.X - A.X @@ C.Y - A.Y
  let r = B.X - A.X @@ B.Y - A.Y
  let s = D.X - C.X @@ D.Y - C.Y
  
  let CmPxr = (CmP.X * r.Y) - (CmP.Y * r.X)
  let CmPxs = (CmP.X * s.Y) - (CmP.Y * s.X)
  let rxs = (r.X * s.Y) - (r.Y * s.X)
  
  if CmPxr = 0.0f then
       ((C.X - A.X < 0.0f) <> (C.X - B.X < 0.0f))
    || ((C.Y - A.Y < 0.0f) <> (C.Y - B.Y < 0.0f))
  else
    if rxs = 0.0f then
      false
    else
      let rxsr = 1.0f / rxs
      let t = CmPxs * rxsr
      let u = CmPxr * rxsr
      
      (t >= 0.0f) && (t <= 1.0f) && (u >= 0.0f) && (u <= 1.0f)