module CircusMaximus.Graphics.PlacingOverlayGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.State

let drawOverlay (fontBatch: SpriteBatch) (screenWidth, screenHeight) (assets: GameContent) players =
  let spacing = float (screenHeight / (List.length players + 2))
  players |> List.iteri
    (fun i (player: Player) ->
      fontBatch.DoWithPointClamp
        (fun fontBatch ->
          let place = match player.finishState with | Finished place -> place | Racing -> 0
          let str = "Locus Histrio " + (i + 1 |> toRoman) + ": " + (toRoman place)
          FlatSpriteFont.drawString
            assets.Font fontBatch str (screenWidth / 2 @@ spacing * (float i + 1.5)) 4.0f Color.White
            (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Max)))