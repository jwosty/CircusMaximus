namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types.UnitSymbols

/// The shape of the racetrack boundries
type RacetrackSpinaShape(center) =
  inherit Polygon(center)
  
  let centerC1 = 42
  
  /// Center points
  override this.Points =
    [-2210, -86;
      // For respawn guides
     -1658, -86;
     -1105, -86;
     //-553, -86;
     // 0, -86;
     // 553, -86;
      1105, -86;
      1658, -86;
      
      2210, -86;
      2280, -66;
      2304,  0;
      2280,  66;
      2210,  86;
      
      // For respawn guides
      1658,  86;
      1105,  86;
     // 553,  86;
     // 0,  86;
     //-553,  86;
     -1105,  86;
     -1658,  86;
      
     -2210,  86;
     -2280,  66;
     -2304,  0;
     -2280, -66 ]
     |> List.map (fun (x, y) -> (x @@ y) * 1.<px> + this.Center)
  
  member this.OuterPoints =
    [ 938 , 702;
      938 , 688;
      // Bend at the end
      8345, 688;
      8472, 720;
      8616, 790;
      8734, 908;
      8826, 1096;
      8843, 1232;
      8826, 1374;
      8734, 1555;
      8616, 1673;
      8345, 1775;
      // Slant
      3122, 1775
      2092, 1609;
      
      769,  1609;
      769,  1594;
      // Starting curve
      617,  1594;
      610,  1475;
      613,  1323;
      623,  1215;
      662,  1018;
      714,  856;
      782,  702; ]
    |> List.map (fun (x, y) -> (x @@ y) * 1.<px>)
  
  member this.RespawnPath =
    this.Points |> List.map (fun p ->
      let direction = (p - center).Normalized
      p + (direction * (250<px> @@ 3000<px>)))
  
  member this.RespawnPathEdges = List.consecutivePairs this.RespawnPath
  
  override this.Edges: CircusMaximus.LineSegment.LineSegment list =
    (List.consecutivePairs this.Points) @ (List.consecutivePairs this.OuterPoints)
  
  override this.Draw(spriteBatch, pixelTexture, redLines) =
    base.Draw(spriteBatch, pixelTexture, redLines)
    this.RespawnPathEdges |> List.iter (fun (start, ``end``) ->
        spriteBatch.Draw(pixelTexture, new Rectangle(start.X - 3.f<px> |> int, start.Y - 3.f<px> |> int, 6, 6), Color.DarkGreen)
        spriteBatch.DrawLine(pixelTexture, xnaVec2 start, xnaVec2 ``end``, Color.Green))