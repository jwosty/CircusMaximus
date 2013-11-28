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
  let placing (player: Player) = match player.finishState with | Finished placing -> placing | Racing -> 0
  players
    |> List.sortBy placing
    |> List.iteri
      (fun i player ->
        fontBatch.DoWithPointClamp
          (fun fontBatch ->
            let str = "Locus " + (player |> placing |> toRoman) + ":      Histrio " + (player.index |> string)
            FlatSpriteFont.drawString
              assets.Font fontBatch str (screenWidth / 2 @@ spacing * (float i + 1.5)) 4.0f
              (player |> placing |> placingColor)
              (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Max)))