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
// updated via copying every frame, it's still lightning fast on OSX. That's some good optimization.
let drawSingle (sb: SpriteBatch) texture x y =
  sb.Draw(texture, (float32 <| x * texture.Width) @~ (float32 <| y * texture.Height))


let collisionShape = new RacetrackSpinaShape(center)
let collisionBounds = BoundingPolygon(collisionShape)

let drawBounds collisionBounds pixelTexture sb = drawUniformBounds pixelTexture sb Color.White collisionBounds