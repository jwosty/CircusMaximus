module CircusMaximus.Graphics.WorldGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let drawWorld ((sb, rect): PlayerScreen.PlayerScreen) assets (settings: GameSettings) (fontBatch: SpriteBatch) players mainPlayer =
  for x in 0..9 do
    for y in 0..2 do
      Racetrack.drawSingle sb assets.RacetrackTextures.[x, y] x y
  if settings.debugDrawBounds
    then Racetrack.drawBounds Racetrack.collisionBounds assets.Pixel sb
  List.iteri
    (fun i player -> PlayerGraphics.drawPlayer (sb, rect) player (i = mainPlayer) settings assets fontBatch)
    players

let drawScreens assets (fontBatch: SpriteBatch) playerScreens settings players =
  List.iteri2
    (PlayerScreen.drawSingle (fun (p, s) -> drawWorld s assets settings fontBatch players p) fontBatch)
    players playerScreens