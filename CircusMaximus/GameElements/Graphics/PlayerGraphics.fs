/// A module to draw player states. It's dirty because it, by nature, has side effects
module CircusMaximus.PlayerGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Player

/// Renders a player, assuming spriteBatch.Begin has already been called
let drawPlayer (sb: SpriteBatch, rect: Rectangle) player isMainPlayer (assets: GameContent) fontBatch =
  let playerBounds, playerIL =
    match player with
    | Moving player -> player.bounds, player.intersectingLines
    | Crashed player -> player.bounds, [false; false; false; false]
  sb.Draw(
    assets.ChariotTexture, playerBounds.Center, new Nullable<_>(), Color.White, single playerBounds.Direction,
    (float32 assets.ChariotTexture.Width / 2.0f @@ float32 assets.ChariotTexture.Height / 2.0f),
    1.0f, // scale
    SpriteEffects.None, single 0)
#if DEBUG
  playerBounds.Draw(sb, assets.Pixel, playerIL)
#endif
  match player with
  | Moving player ->
    // Draw the player's taunt, if any
    match player.currentTaunt with
    | Some taunt ->
      FlatSpriteFont.drawString
        assets.Font fontBatch taunt player.position 2.0f
        (if isMainPlayer then Color.White else Color.OrangeRed)
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed player ->
    // Remind the player that they are crashed
    let message, color = if isMainPlayer then "Strepebas!", Color.Red else "Strepebat!", (new Color(Color.Red, 63))
    FlatSpriteFont.drawString
      assets.Font fontBatch message player.position 3.0f color
      (FlatSpriteFont.Center, FlatSpriteFont.Center)