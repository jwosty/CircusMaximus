module CircusMaximus.HUDGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.State

/// Draws the top portion of the HUD, which contains information about the player and how they are doing
let drawInfo fontBatch (player: Player) ((sb, rect): PlayerScreen.PlayerScreen) (assets: GameContent) =
  let drawString str vertPos color =
    FlatSpriteFont.drawString
      assets.Font fontBatch str (float rect.X + (float rect.Width / 2.0) @@ (float rect.Y + vertPos * 24.0))
      3.f color (FlatSpriteFont.Center, FlatSpriteFont.Min)
  match player.motionState with
  | Moving velocity ->
    drawString ("Histrio " + (player.number |> toRoman)) 0. Color.White
    drawString ("Flexus: " + (MathHelper.Clamp(player.turns, 0, Int32.MaxValue) |> toRoman)) 1. Color.White
    match player.finishState with
    | Finished placing ->
        drawString
          (sprintf "Locus: %s" (toRoman placing)) 2.
          (placingColor placing)
    | Racing -> ()
  | Crashed -> ()

/// Draws the bottom portion of the HUD (player items)
let drawItems (generalBatch: SpriteBatch) fontBatch items ((sb, rect): PlayerScreen.PlayerScreen) assets =
  generalBatch.DoBasic (fun generalBatch ->
    let spacing = float32 rect.Width / float32 (List.length items + 1)
    items |> List.iteri (fun i item ->
      let slot = float32 (List.length items) / 2.f - float32 i
      let x = float32 rect.X + (float32 rect.Width / 2.f) + (slot * float32 ItemGraphics.defaultItemImageWidth)
      let y = (rect.Y + rect.Height - ItemGraphics.defaultItemImageHeight)
      ItemGraphics.draw generalBatch item (x @@ y) assets))

/// Draws a player's HUD (Heads-Up Display)
let draw generalBatch fontBatch assets player playerScreen =
  drawInfo fontBatch player playerScreen assets
  drawItems generalBatch fontBatch player.items playerScreen assets