module CircusMaximus.LineSegment
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

type LineSegment = Vector2 * Vector2

/// Tests if two line segments intersect (maths come from http://stackoverflow.com/a/565282/1231925)
let (-+-) ((p, pr): LineSegment) ((q, qs): LineSegment) =
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

/// Draws a line segment. Simple.
let drawLineSegment texture (sb: SpriteBatch) color (seg: LineSegment) = sb.DrawLine(texture, fst seg, snd seg, color)