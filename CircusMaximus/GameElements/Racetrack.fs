module CircusMaximus.Racetrack
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Collision

let center = 5418 @@ 1255

let loadContent (content: ContentManager) =
  // Use a 2D array because there aren't 2D lists, and a 1D list would be harder to deal with here
  // The GIMP plugin that split the image generates the files in the format y-x.png -- I should fix
  // that sometime
  Array2D.init 10 3 (fun x y -> content.Load<Texture2D>(sprintf "racetrack/%i-%i.png" y x))

// Wow, even though the textures collectively contain about 25 million pixels, and the players are
// updated via copying every fram, it's still lightning fast on OSX. That's some good optimization.
let drawSingle (sb: SpriteBatch) (texture: Texture2D) x y =
  sb.Draw(texture, (float32 <| x * texture.Width) @~ (float32 <| y * texture.Height))

let collisionBounds =
  // Tiles are 975x811 pixels
  // 282, 358
  // 803, 530
  // Center rectangle
  [BoundingPolygon(new RacetrackSpinaShape(center));
   BoundingPolygon(new RacetrackOuterShape())]

let drawBounds collisionBounds pixelTexture sb = collisionBounds |> List.iter (drawUniformBounds pixelTexture sb Color.White)