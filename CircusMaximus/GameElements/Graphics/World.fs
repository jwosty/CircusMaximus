module CircusMaximus.Graphics.WorldGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

let draw (graphics: GraphicsDeviceManager) (spriteBatch: SpriteBatch) assets (settings: GameSettings) (fontBatch: SpriteBatch) (players: Player list) =
  // Build transformation matrix
  let xs, ys = players |> List.map (fun player -> player.position.X), players |> List.map (fun player -> player.position.Y)
  let center = (List.min xs + List.max xs) / 2.f @@ (List.min ys + List.max ys) / 2.f
  let translation = (graphics.PreferredBackBufferWidth * 1<px> @@ graphics.PreferredBackBufferHeight * 1<px>) / 2 - center
  let matrix = Matrix.CreateTranslation (new Vector3(float32 translation.X, float32 translation.Y, 0.f))
  spriteBatch.Begin (SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, matrix)
  fontBatch.Begin (SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, matrix)
  RacetrackGraphics.draw assets spriteBatch
  if settings.debugDrawBounds
    then RacetrackGraphics.drawBounds Racetrack.collisionBounds assets.Pixel spriteBatch
  List.iteri
    (fun i player -> PlayerGraphics.drawPlayer spriteBatch player settings assets fontBatch)
    players
  spriteBatch.End ()
  fontBatch.End ()