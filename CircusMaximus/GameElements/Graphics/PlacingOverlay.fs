module CircusMaximus.Graphics.PlacingOverlayGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types

/// Draws the play placing overlay. Does not call (but needs) SpriteBatch.Begin and SpriteBatch.End
let drawOverlay (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (screenWidth, screenHeight) (assets: GameContent) players =
  let padding = 16
  let spacing = float (screenHeight / (List.length players + padding))
  let placing (player: Player) = match player.finishState with | Finished placing -> placing | Racing -> 0
  players
    |> List.sortBy placing
    |> List.iteri
      (fun i player ->
        let center = (screenWidth / 2) @@ (spacing * (float i + (float padding * 0.5)))
        generalBatch.DrawCentered(assets.PlacingBackground, center - (0 @@ 3), new Color(Color.White, 255))
        let str = "Locus " + (player |> placing |> toRoman) + ":      Auriga " + player.colorString
        FlatSpriteFont.drawString
          assets.Font fontBatch str center 4.0f
          (player |> placing |> placingColor)
          (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center))