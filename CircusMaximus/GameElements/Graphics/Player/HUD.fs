module CircusMaximus.HUDGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Types

(*
/// Draws the top portion of the HUD, which contains information about the player and how they are doing
let drawInfo fontBatch (player: Player) (assets: GameContent) =
  let drawString str vertPos color =
    FlatSpriteFont.drawString
      assets.Font fontBatch str (float rect.X + (float rect.Width / 2.0) @@ (float rect.Y + vertPos * 24.0))
      3.f color (FlatSpriteFont.Center, FlatSpriteFont.Min)
  match player.motionState with
  | Moving(spawnTime, velocity) ->
    drawString ("Auriga " + player.colorString) 0. player.color
    drawString ("Flexus: " + (MathHelper.Clamp(player.turns, 0, Int32.MaxValue) |> toRoman)) 1. Color.White
    match player.finishState with
    | Finished placing ->
        drawString
          (sprintf "Locus: %s" (toRoman placing)) 2.
          (placingColor placing)
    | Racing -> ()
  | Crashed _ -> ()
*)

/// Draws the bottom portion of the HUD (player items)
let drawItemBar (generalBatch: SpriteBatch) fontBatch (player: Player) assets =
  //let spacing = float32 rect.Width / float32 (player.items.Length + 1)
  let widthDiff = ItemGraphics.defaultItemSelectorImageWidth - ItemGraphics.defaultItemImageWidth
  let heightDiff = ItemGraphics.defaultItemSelectorImageHeight - ItemGraphics.defaultItemImageHeight
  player.items |> List.iteri (fun i item ->
    let i' = float32 i - (float32 player.items.Length / 2.f)
    let itemCenter = (player.position.X + (float32 ItemGraphics.defaultItemSelectorImageWidth * i')) @@ (player.position.Y - player.bounds.Width)
    ItemGraphics.draw generalBatch item itemCenter assets
    if player.selectedItem = i
      then generalBatch.DrawCentered(assets.ItemSelector, itemCenter, Color.White))

/// Draws a player's HUD (Heads-Up Display)
let draw generalBatch fontBatch assets player = drawItemBar generalBatch fontBatch player assets