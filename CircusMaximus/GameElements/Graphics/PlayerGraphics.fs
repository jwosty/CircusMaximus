/// A module to draw player states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.PlayerGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.State

/// Renders a player, assuming spriteBatch.Begin has already been called
let drawPlayer (sb: SpriteBatch, rect: Rectangle) (player: Player) isMainPlayer (assets: GameContent) fontBatch =
  sb.Draw(
    assets.ChariotTexture, player.position, new Nullable<_>(), Color.White, single player.direction,
    (float32 assets.ChariotTexture.Width / 2.0f @@ float32 assets.ChariotTexture.Height / 2.0f),
    1.0f, // scale
    SpriteEffects.None, single 0)
#if DEBUG
  player.bounds.Draw(sb, assets.Pixel, player.intersectingLines)
#endif
  match player.motionState with
  | Moving velocity ->
    // Draw the player's taunt, if any
    match player.tauntState with
    | Some(taunt, _) ->
      FlatSpriteFont.drawString
        assets.Font fontBatch taunt player.position 2.0f
        (if isMainPlayer then Color.White else Color.OrangeRed)
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed ->
    // Remind the player that they are crashed
    let message, color = if isMainPlayer then "Strepebas!", Color.Red else "Strepebat!", (new Color(Color.Red, 63))
    FlatSpriteFont.drawString
      assets.Font fontBatch message player.position 3.0f color
      (FlatSpriteFont.Center, FlatSpriteFont.Center)