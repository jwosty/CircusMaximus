module CircusMaximus.Graphics.PlacingOverlayGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

/// Draws the play placing overlay. Does not call (but needs) SpriteBatch.Begin and SpriteBatch.End
let drawOverlay (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (screenDimensions: Vector2<px>) (assets: GameContent) players =
  let padding = 16
  let spacing = float (screenDimensions.Y / float32 (List.length players + padding)) * 1.<px>
  let placing (player: Player) = match player.finishState with | Finished placing -> placing | Racing -> 0
  players
    |> List.sortBy placing
    |> List.iteri
      (fun i player ->
        let center = (screenDimensions.X / 2.f) @@ (spacing * (float i + (float padding * 0.5)))
        generalBatch.DrawCentered(assets.PlacingBackground, center - (0<px> @@ 3<px>) |> xnaVec2, new Color(Color.White, 255))
        let str = "Locus " + (player |> placing |> toRoman) + ":      Auriga " + player.colorString
        FlatSpriteFont.drawString
          assets.Font fontBatch str center 4.0
          (player |> placing |> placingColor)
          (FlatSpriteFont.Alignment.Center, FlatSpriteFont.Alignment.Center))