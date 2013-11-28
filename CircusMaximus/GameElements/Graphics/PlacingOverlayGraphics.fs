module CircusMaximus.Graphics.PlacingOverlayGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.State

let drawOverlay (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (screenWidth, screenHeight) (assets: GameContent) players =
  let spacing = float (screenHeight / (List.length players + 2))
  let placing (player: Player) = match player.finishState with | Finished placing -> placing | Racing -> 0
  players
    |> List.sortBy placing
    |> List.iteri
      (fun i player ->
        let center = screenWidth / 2 @@ spacing * (float i + 1.25)
        generalBatch.DoBasic (fun generalBatch -> generalBatch.DrawCentered(assets.PlacingBackground, center - (0 @@ 3), new Color(Color.White, 255)))
        fontBatch.DoWithPointClamp
          (fun fontBatch ->
            let str = "Locus " + (player |> placing |> toRoman) + ":      Histrio " + (player.index |> toRoman)
            FlatSpriteFont.drawString
              assets.Font fontBatch str center 4.0f
              (player |> placing |> placingColor)
              (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center)))