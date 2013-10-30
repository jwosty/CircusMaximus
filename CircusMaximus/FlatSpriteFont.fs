// Renders monospaced, square character bitmap fonts with only one horizontal layer of images using SpriteBatch
module CircusMaximus.FlatSpriteFont
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

// Returns the a rectangle that specifies where the character is in the texture
let getCharTextureRectangle (chr: char) (w, h) =
  // Convert the ASCII char to an index (32 < ASCII value because that's where the printable chars begin)
  new Rectangle(int chr - 32 * w, 0, w, h)

// Draw a character
let drawChar (fontTexture: Texture2D) (sb: SpriteBatch) (chr: char) (position: Vector2) scale (color: Color) =
  sb.Draw(fontTexture,
    new Rectangle(int position.X, int position.Y, fontTexture.Height * scale, fontTexture.Height * scale),
    new Nullable<_>(new Rectangle((int chr - 32) * fontTexture.Height, 0, fontTexture.Height, fontTexture.Height)),
    color)
  (*
  sb.Draw(fontTexture,
    new Rectangle(int position.X, int position.Y, scale, scale),
    new Nullable<_>(getCharTextureRectangle chr (fontTexture.Height, fontTexture.Height)),
    color)
  *)