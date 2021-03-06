module CircusMaximus.Extensions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

type Microsoft.Xna.Framework.Graphics.SpriteFont with
  member this.MeasureStringPx (text: string) = new Vector2<_>(this.MeasureString (text))

type Microsoft.Xna.Framework.Graphics.SpriteBatch with
  member this.DrawStringCentered (font: SpriteFont, string: string, position, color) =

    this.DrawString (font, string, font.MeasureStringPx(string) / (2<px> @@ 2<px>) |> xnaVec2, color)
  
  member this.DrawLine(t: Texture2D, start: Vector2, ``end``: Vector2, ?color, ?width) =
    let width_, color_ = defaultArg width 1, defaultArg color Color.White
    let edge = ``end`` - start
    let blah = atan2 1. 2.
    this.Draw(t,
      new Rectangle(int start.X, int start.Y, int <| edge.Length(), width_),
      new Nullable<Rectangle>(), color_,
      atan2 edge.Y edge.X |> float32,  // angle
      Vector2.Zero, SpriteEffects.None, 0.0f)
  
  /// Begins the sprite batch with no arguments and calls the predicate with the sprite batch
  /// before ending
  member this.DoBasic(predicate) =
    this.Begin (SpriteSortMode.Deferred, BlendState.NonPremultiplied)
    predicate this |> ignore
    this.End ()
  
  member this.DrawCentered(texture: Texture2D, center, color) =
    this.Draw(texture, center - new Vector2(float32 texture.Width / 2.f, float32 texture.Height / 2.f), color)

  /// Begins the sprite batch in point clamp mode and calls the predicate with the sprite batch
  /// before ending
  member this.DoWithPointClamp(predicate) =
    // SamplerState.PointClamp disables anti-aliasing, which just looks horrible on scaled bitmap fonts
    this.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null)
    predicate this |> ignore
    this.End()

module List =
  /// Returns the consecutive pairs of a list (including the first and last elements together)
  let consecutivePairs list =
    let max = List.length list - 1
    List.mapi
      (fun i _ ->
        let nextI = if i = max then 0 else i + 1
        list.[i], list.[nextI])
      list
  
  /// Returns a list without the element at the index
  let removeIndex index list =
    let mutable result = []
    let mutable erodingList = list
    for i in 0 .. List.length list - 1 do
      if i <> index then
        result <- result @ [erodingList.Head]
      erodingList <- erodingList.Tail
    result
  
  /// Merges multiple lists into one
  let merge (lists: 'a list list) =
    lists
      |> Seq.concat
      |> Seq.distinctBy id
      |> List.ofSeq
  
  /// Four-way zip
  let zip4 a (b: _ list) (c: _ list) (d: _ list) = List.init (List.length a) (fun i -> a.[i], b.[i], c.[i], d.[i])
  
  /// Combines any number of lists using the given predicate
  let rec combine predicate (lists: 'a list list) =
    List.init
      (lists.[0].Length)
      (fun i ->
        lists
          |> List.map (fun list -> list.[i])
          |> List.reduce predicate)
  
  /// Returns a list containing all non-duplicate pairs of objects possible
  let rec pairCombinations list =
    match list with
      | [] -> []
      | [a; b] -> [a, b]
      | head :: tail -> (List.map (fun x -> head, x) tail) @ pairCombinations tail
  
  let rec skip times list =
    if times < 1 then list
    else skip (times - 1) (List.tail list)
  
  let inline wrapIndex (list: _ list) i =
    let wi = i % (list.Length)
    if wi < 0
      then list.Length + wi
      else wi
  
  let inline addIf cond createThing list = if cond then createThing() :: list else list
  let inline appendFrontIf cond createThings list = if cond then createThings() @ list else list