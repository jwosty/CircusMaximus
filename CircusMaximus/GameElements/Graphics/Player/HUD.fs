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
let drawItems (generalBatch: SpriteBatch) fontBatch (player: Player) ((sb, rect): PlayerScreen.PlayerScreen) assets =
  generalBatch.DoBasic (fun generalBatch ->
    let spacing = float32 rect.Width / float32 (player.items.Length + 1)
    let widthDiff = ItemGraphics.defaultItemSelectorImageWidth - ItemGraphics.defaultItemImageWidth
    let heightDiff = ItemGraphics.defaultItemSelectorImageHeight - ItemGraphics.defaultItemImageHeight
    player.items |> List.iteri (fun i item ->
      let slot = float32 player.items.Length / 2.f - float32 i
      let x = float32 rect.X + (float32 rect.Width / 2.f) + (slot * (float32 ItemGraphics.defaultItemSelectorImageWidth / 2.f))
      let y = (float32 rect.Y + float32 rect.Height - (float32 ItemGraphics.defaultItemSelectorImageHeight / 2.f))
      ItemGraphics.draw generalBatch item (x @@ y) assets
      if player.selectedItem = i
        then generalBatch.DrawCentered(assets.ItemSelector, x @@ y, Color.White)))

/// Draws a player's HUD (Heads-Up Display)
let draw generalBatch fontBatch assets player playerScreen =
  drawInfo fontBatch player playerScreen assets
  drawItems generalBatch fontBatch player playerScreen assets