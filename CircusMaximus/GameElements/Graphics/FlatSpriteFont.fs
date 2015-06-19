// Renders monospaced, square character bitmap fonts with only one horizontal layer of images using SpriteBatch
module CircusMaximus.FlatSpriteFont
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

type Alignment = | Min | Center | Max

type AxisAlignment = Alignment * Alignment

// Returns a vector of the offset for an object to be aligned in that manner
let inline axisAlignmentToOffset (objWidth: float<px>) (objHeight: float<px>) ((xAlignment, yAlignment): AxisAlignment) =
  let x =
    match xAlignment with
      | Min -> 0.0 | Center -> -0.5 | Max -> -1.0
      * objWidth
  let y =
    match yAlignment with
      | Min -> 0.0 | Center -> -0.5 | Max -> -1.0
      * objHeight
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

let width (str: string) (charWidth: float<px>) scale : float<px>=
  float (str.Split([|'\n'|]) |> Array.fold (fun longest str -> if str.Length > longest then str.Length else longest) 0)
    * charWidth * scale

let height (str: string) (charHeight: float<px>) scale : float<px> = float (str.Split([|'\n'|]).Length) * charHeight * scale

// Draws a string without interpreting newlines
let drawSingleLineString fontTexture sb (str: String) position scale color =
  Array.iteri
    (fun i chr ->
      drawChar
        fontTexture sb chr
        // line position + char offset
        (xnaVec2 (position + (float fontTexture.Height * 1.<px> * float scale * float i @@ 0<px>)))
        scale color)
    (str.ToCharArray())

let drawString (fontTexture: Texture2D) sb (str: String) position scale color alignment =
  let grandOffset = axisAlignmentToOffset <| width str (float fontTexture.Height * 1.<px>) scale <| height str (float fontTexture.Height * 1.<px>) scale <| alignment
  Array.iteri
    (fun i siStr ->
      let p = (0<px> @@ float fontTexture.Height * 1.<px> * float scale * float i)
      
      drawSingleLineString
        fontTexture sb siStr
        // string position + line offset + string offset
        (position + (0<px> @@ float fontTexture.Height * 1.<px> * float scale * float i) + grandOffset)
        scale color)
    (str.Split([|'\n'|]))