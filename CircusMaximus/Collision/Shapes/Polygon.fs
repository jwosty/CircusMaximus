namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions

[<AbstractClass>]
type Polygon(center) =
  member this.Center: Vector2 = center
  
  /// A clockwise list of the polygon's vertices
  abstract member Points: Vector2 list
  
  /// Decomposes the structure into the most primitive parts (line segments)
  abstract member Edges: CircusMaximus.LineSegment.LineSegment list
  default this.Edges: CircusMaximus.LineSegment.LineSegment list = this.Points |> List.consecutivePairs
  
  /// Draws edges
  member this.Draw(sb: SpriteBatch, pixelTexture, redLines) =
    List.iter2
      (fun isRed (start, ``end``) ->
        let color = if isRed then Color.Red else Color.White
        sb.DrawLine(pixelTexture, start, ``end``, color))
      redLines
      this.Edges