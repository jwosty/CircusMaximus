namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open CircusMaximus.HelperFunctions

/// The shape of the racetrack boundries
type RacetrackShape(center) =
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