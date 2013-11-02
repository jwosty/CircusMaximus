// Renders monospaced, square character bitmap fonts with only one horizontal layer of images using SpriteBatch
module CircusMaximus.FlatSpriteFont
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open HelperFunctions

type Alignment = | Min | Center | Max

type AxisAlignment = Alignment * Alignment

// Returns a vector of the offset for an object to be aligned in that manner
let inline axisAlignmentToOffset objWidth objHeight ((xAlignment, yAlignment): AxisAlignment) =
  let x =
    match xAlignment with
      | Min -> 0.0f | Center -> -0.5f | Max -> -1.0f
      * float32 objWidth
  let y =
    match yAlignment with
      | Min -> 0.0f | Center -> -0.5f | Max -> -1.0f
      * float32 objHeight
  x @@ y

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
  float32 (str.Split([|'\n'|]) |> Array.fold (fun longest str -> if str.Length > longest then str.Length else longest) 0)
    * float32 charWidth * scale

let height (str: string) charHeight scale =
  float32 (str.Split([|'\n'|]).Length) * float32 charHeight * scale

// Draws a string without interpreting newlines
let drawSingleLineString fontTexture sb (str: String) position scale color =
  Array.iteri
    (fun i chr ->
      drawChar
        fontTexture sb chr
        // line position + char offset
        (position + (float32 fontTexture.Height * scale * float32 i @@ 0))
        scale color)
    (str.ToCharArray())

let drawString (fontTexture: Texture2D) sb (str: String) position scale color alignment =
  let grandOffset = axisAlignmentToOffset <| width str fontTexture.Height scale <| height str fontTexture.Height scale <| alignment
  Array.iteri
    (fun i siStr ->
      drawSingleLineString
        fontTexture sb siStr
        // string position + line offset + string offset
        (position + (0 @@ float32 fontTexture.Height * scale * float32 i) + grandOffset)
        scale color)
    (str.Split([|'\n'|]))