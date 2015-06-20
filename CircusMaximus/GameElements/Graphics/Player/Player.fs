/// A module to draw player states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.PlayerGraphics
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.Functions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

/// Render a player without calling any SpriteBatch#Begin or SpriteBatch#End
let drawPlayer (spriteBatch: SpriteBatch) (player: Player) (settings: GameSettings) (assets: GameContent) (fontBatch: SpriteBatch) =
  let playerAlpha, shouldDrawGlow =
    match player.motionState with
    | Moving(Spawning spawnTime, _, _) ->
      if (int spawnTime) % 15 < 5
        then 255, true
        else 128, false
    | _ -> 255, true
  if shouldDrawGlow then
    // Draw a glow to show the player's color
    spriteBatch.Draw(
      assets.PlayerGlow, xnaVec2 player.position, new Nullable<_>(), player.color, float32 player.direction,
      new Vector2(float32 assets.PlayerGlow.Width / 2.f, float32 assets.PlayerGlow.Height / 2.f),
      1.0f, SpriteEffects.None, 0.f)
  // Draw the chariot
  spriteBatch.Draw(
    assets.ChariotTexture, xnaVec2 player.position, new Nullable<_>(), new Color(Color.White, playerAlpha), float32 player.direction,
    new Vector2(float32 assets.ChariotTexture.Width / 2.f, float32 assets.ChariotTexture.Height / 2.f),
    1.0f, SpriteEffects.None, 0.f)
  
  if settings.debugDrawBounds
    then player.bounds.Draw(spriteBatch, assets.Pixel, player.intersectingLines)
  
  // Draw particles
  player.particles |> List.iter
    (fun p ->
      let fade = (BoundParticle.particleAge - p.age) / BoundParticle.particleAge |> float32 // Particles fade out
      spriteBatch.DrawCentered(assets.Particle, xnaVec2 (player.position + p.position), Color.White * fade))
  
  if settings.debugDrawAccelerationTimer then
    FlatSpriteFont.drawString assets.Font fontBatch (((float player.accelerationTimer).ToString "0.00") + "s") player.position 2.0 Color.White (FlatSpriteFont.Center, FlatSpriteFont.Center)

  
  match player.motionState with
  | Moving(_, _, _) ->
    // Draw the player's taunt, if any
    match player.tauntState with
    | Some(taunt, duration) ->
      let fade = (float32 duration) / (float32 EffectDurations.taunt)
      FlatSpriteFont.drawString
        assets.Font fontBatch taunt player.position 2.0
        (Color.White * ((float32 duration) / (float32 EffectDurations.taunt))) // Fading text
        (FlatSpriteFont.Center, FlatSpriteFont.Center)
    | None -> ()
  | Crashed timeCrashed ->
    // Indicate a crashed player
    FlatSpriteFont.drawString
      assets.Font fontBatch "Strepebat!" player.position 3.0 Color.Red
      (FlatSpriteFont.Center, FlatSpriteFont.Center)
  HUDGraphics.draw spriteBatch fontBatch assets player