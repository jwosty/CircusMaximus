// Renders monospaced, square character bitmap fonts with only one horizontal layer of images using SpriteBatch
module CircusMaximus.FlatSpriteFont
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

// Returns the a rectangle that specifies where the character is in the texture
let getCharTextureRectangle (chr: char) (w, h) =
  // Convert the ASCII char to an index (32 < ASCII value because that's where the printable chars begin)
  new Rectangle(int chr - 32 * w, 0, w, h)

// Draw a character. Will crash if the character is < ASCII 32
let drawChar (fontTexture: Texture2D) (sb: SpriteBatch) (chr: char) (position: Vector2) scale (color: Color) =
  sb.Draw(fontTexture,
    new Rectangle(int position.X, int position.Y, fontTexture.Height * int scale, fontTexture.Height * int scale),
    new Nullable<_>(new Rectangle((int chr - 32) * fontTexture.Height, 0, fontTexture.Height, fontTexture.Height)),
    color)

// Draws a string without interpreting newlines
let drawSingleLineString fontTexture sb (str: String) position scale color =
  str.ToCharArray() |> Array.iteri (fun i chr -> drawChar fontTexture sb chr (position + new Vector2(float32 <| fontTexture.Height * scale * i, 0.0f)) scale color)

let drawString fontTexture sb (str: String) position scale color =
  str.Split([|'\n'|]) |> Array.iteri (fun i siStr -> drawSingleLineString fontTexture sb siStr (position + new Vector2(0.0f, float32 <| fontTexture.Height * scale * i)) scale color)