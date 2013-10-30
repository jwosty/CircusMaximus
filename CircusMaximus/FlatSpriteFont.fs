// Renders monospaced, square character bitmap fonts with only one horizontal layer of images using SpriteBatch
module CircusMaximus.FlatSpriteFont
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type OriginAlignment = | CenterX | CenterY | CenterXCenterY | XY

// Returns a vector of the offset for an object to be aligned in that manner
let inline alignmentToOffset objWidth objHeight alignment =
  match alignment with
    | CenterX -> new Vector2(-0.5f, 0.0f)
    | CenterY -> new Vector2(0.0f, -0.5f)
    | CenterXCenterY -> new Vector2(-0.5f, -0.5f)
    | XY -> new Vector2(0.0f, 0.0f)
    * new Vector2(float32 objWidth, float32 objHeight)

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

// Returns an list of the string's lines
let lines (str: string) = List.ofArray (str.Split([|'\n'|]))

let width (str: string) charWidth scale =
  (str.Split([|'\n'|]) |> Array.fold (fun longest str -> if str.Length > longest then str.Length else longest) 0)
    * charWidth * scale

let height (str: string) charHeight scale =
  str.Split([|'\n'|]).Length
    * charHeight * scale

// Draws a string without interpreting newlines
let drawSingleLineString fontTexture sb (str: String) position scale color =
  Array.iteri
    (fun i chr ->
      drawChar
        fontTexture sb chr
        // line position + char offset
        (position + new Vector2(float32 <| fontTexture.Height * scale * i, 0.0f))
        scale color)
    (str.ToCharArray())

let drawString (fontTexture: Texture2D) sb (str: String) position scale color alignment =
  let grandOffset = alignmentToOffset <| width str fontTexture.Height scale <| height str fontTexture.Height scale <| alignment
  Array.iteri
    (fun i siStr ->
      drawSingleLineString
        fontTexture sb siStr
        // string position + line offset + string offset
        (position + new Vector2(0.0f, float32 <| fontTexture.Height * scale * i) + grandOffset)
        scale color)
    (str.Split([|'\n'|]))