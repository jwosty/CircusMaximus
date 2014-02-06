/// A module to draw player states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.PlayerGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.State

/// Renders a player, assuming spriteBatch.Begin has already been called
let drawPlayer (sb: SpriteBatch, rect: Rectangle) (player: Player) isMainPlayer (settings: GameSettings) (assets: GameContent) fontBatch =
  let playerAlpha, shouldDrawGlow =
    match player.motionState with
    | Moving(Spawning spawnTime, _) ->
      if spawnTime % 15 < 5
        then 255, true
        else 128, false
    | _ -> 255, true
  if shouldDrawGlow then
    // Draw a glow to show the player's color
    sb.Draw(
      assets.PlayerGlow, player.position, new Nullable<_>(), player.color, float32 player.direction,
      (float32 assets.PlayerGlow.Width / 2.0f @@ float32 assets.PlayerGlow.Height / 2.0f),
      1.0f, SpriteEffects.None, 0.f)
  // Draw the chariot
  sb.Draw(
    assets.ChariotTexture, player.position, new Nullable<_>(), new Color(Color.White, playerAlpha), float32 player.direction,
    (float32 assets.ChariotTexture.Width / 2.0f @@ float32 assets.ChariotTexture.Height / 2.0f),
    1.0f, SpriteEffects.None, 0.f)
  
  if settings.debugDrawBounds
    then player.bounds.Draw(sb, assets.Pixel, player.intersectingLines)
  
  // Draw particles
  player.particles |> List.iter
    (fun p ->
      let fade = (BoundParticle.particleAge - p.age) / BoundParticle.particleAge |> float32 // Particles fade out
      sb.DrawCentered(assets.Particle, player.position + p.position, Color.White * fade))
  
  match player.motionState with
  | Moving(_, velocity) ->
    // Draw the player's taunt, if any
    match player.tauntState with
    | Some(taunt, duration) ->
      let fade = (float32 duration) / (float32 EffectDurations.taunt)
      FlatSpriteFont.drawString
        assets.Font fontBatch taunt player.position 2.0f
        (if isMainPlayer then Color.White * fade else Color.OrangeRed * fade)   // Get the color and fade it out depending on how long the player has been taunting
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed timeCrashed ->
    // Remind the player that they are crashed
    let message, color = if isMainPlayer then "Strepebas!", Color.Red else "Strepebat!", (new Color(Color.Red, 63))
    FlatSpriteFont.drawString
      assets.Font fontBatch message player.position 3.0f color
      (FlatSpriteFont.Center, FlatSpriteFont.Center)