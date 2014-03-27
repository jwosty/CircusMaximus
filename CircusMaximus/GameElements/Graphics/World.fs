module CircusMaximus.Graphics.WorldGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types

let draw ((sb, rect): PlayerScreen.PlayerScreen) assets (settings: GameSettings) (fontBatch: SpriteBatch) players mainPlayer =
  RacetrackGraphics.draw assets sb
  if settings.debugDrawBounds
    then RacetrackGraphics.drawBounds Racetrack.collisionBounds assets.Pixel sb
  List.iteri
    (fun i player -> PlayerGraphics.drawPlayer (sb, rect) player (i = mainPlayer) settings assets fontBatch)
    players

let drawScreens assets (fontBatch: SpriteBatch) playerScreens settings players =
  List.iteri2
    (PlayerScreen.drawSingle (fun (p, s) -> draw s assets settings fontBatch players p) fontBatch)
    players playerScreens