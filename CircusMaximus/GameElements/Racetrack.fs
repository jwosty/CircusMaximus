module CircusMaximus.Racetrack
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

let center = 5418 @@ 1255

// Wow, even though the textures collectively contain about 25 million pixels, and the players are
// updated via copying every fram, it's still lightning fast on OSX. That's some good optimization.
let drawSingle (sb: SpriteBatch) (texture: Texture2D) x y =
  sb.Draw(texture, (float32 <| x * texture.Width) @~ (float32 <| y * texture.Height))

let collisionBounds =
  // Tiles are 975x811 pixels
  // 282, 358
  // 803, 530
  // Center rectangle
  BoundingPolygon(new RacetrackSpinaShape(center))

let drawBounds collisionBounds pixelTexture sb = drawUniformBounds pixelTexture sb Color.White collisionBounds