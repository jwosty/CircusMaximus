namespace CircusMaximus
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

[<AbstractClass>]
type Polygon(center) =
  member this.Center: Vector2<px> = center
  
  /// A clockwise list of the polygon's vertices
  abstract member Points: Vector2<px> list
  
  /// Decomposes the structure into the most primitive parts (line segments)
  abstract member Edges: CircusMaximus.LineSegment.LineSegment list
  default this.Edges: CircusMaximus.LineSegment.LineSegment list = this.Points |> List.consecutivePairs
  
  /// Draws edges
  abstract member Draw: SpriteBatch * Texture2D * bool list -> unit
  default this.Draw(sb: SpriteBatch, pixelTexture, redLines) =
    List.iter2
      (fun isRed (start, ``end``) ->
        let color = if isRed then Color.Red else Color.White
        sb.DrawLine(pixelTexture, start, ``end``, color))
      redLines
      (this.Edges |> List.map (fun (start,``end``) -> xnaVec2 start, xnaVec2 ``end``))