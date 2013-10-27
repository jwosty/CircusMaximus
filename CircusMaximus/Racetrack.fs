module CircusMaximus.Racetrack
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

let loadContent (content: ContentManager) =
  // Use a 2D array because there aren't 2D lists, and a 1D list would be harder to deal with here
  // The GIMP plugin that split the image generates the files in the format y-x.png -- I should fix
  // that sometime
  Array2D.init 12 3 (fun x y -> content.Load<Texture2D>(sprintf "racetrack/%i-%i.png" y x))

// Wow, even though the textures collectively contain about 40,000,000 pixels, and the players are
// updated by copying, it's still lightning fast on OSX. That's some good optimization.
let drawSingle (sb: SpriteBatch) texture x y =
  // Every is tile 1044x1043 pixels
  sb.Draw(texture, new Vector2(float32 <| x * 1044, float32 <| y * 1043), Color.White)