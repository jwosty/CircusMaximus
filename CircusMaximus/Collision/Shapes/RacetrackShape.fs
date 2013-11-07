namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions

/// The shape of the racetrack boundries
type RacetrackSpinaShape(center) =
  inherit Polygon(center)
  
  override this.Points =
    [-2210 @@ -86;
      2210 @@ -86;
      2280 @@ -66;
      2304 @@  0;
      2280 @@ 66;
      2210 @@  86;
     -2210 @@  86;
     -2280 @@  66;
     -2304 @@  0;
     -2280 @@ -66]
     |> List.map ((+) this.Center)

type RacetrackOuterShape() =
  inherit Polygon(0 @@ 0)
  
  override this.Points =
    [938  @@ 702;
     938  @@ 688;
     // Bend at the end
     8345 @@ 688;
     8472 @@ 720;
     8616 @@ 790;
     8734 @@ 908;
     8826 @@ 1096;
     8843 @@ 1232;
     8826 @@ 1374;
     8734 @@ 1555;
     8616 @@ 1673;
     8345 @@ 1775;
     // Slant
     3122 @@ 1775
     2092 @@ 1609;
     
     769  @@ 1609;
     769  @@ 1594;
     // Starting curve
     617  @@ 1594;
     610  @@ 1475;
     613  @@ 1323;
     623  @@ 1215;
     662  @@ 1018;
     714  @@ 856;
     782  @@ 702;
     ]