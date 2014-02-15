module CircusMaximus.Graphics.RacetrackGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Collision

/// Renders the racetrack
let draw (assets: GameContent) (spriteBatch: SpriteBatch) =
  for x in 0..9 do
    for y in 0..2 do
      let texture = assets.RacetrackTextures.[x, y]
      spriteBatch.Draw(texture, (float32 <| x * texture.Width) @~ (float32 <| y * texture.Height))

/// Draws the racetrack bounds
let drawBounds collisionBounds pixelTexture sb = drawUniformBounds pixelTexture sb Color.White collisionBounds