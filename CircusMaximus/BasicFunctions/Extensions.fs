module CircusMaximus.Extensions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions

type Microsoft.Xna.Framework.Graphics.SpriteBatch with
  member this.DrawStringCentered(font: SpriteFont, string: string, position, color) =
    this.DrawString(font, string, font.MeasureString(string) / (2 @@ 2), color)
  
  member this.DrawLine(t: Texture2D, start: Vector2, ``end``: Vector2, ?color, ?width) =
    let width_, color_ = defaultArg width 1, defaultArg color Color.White
    let edge = ``end`` - start
    this.Draw(t,
      new Rectangle(int start.X, int start.Y, int <| edge.Length(), width_),
      new Nullable<Rectangle>(), color_,
      atan2 edge.Y edge.X,  // angle
      0 @@ 0, SpriteEffects.None, 0.0f)

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
    // Written in the imperitive style for the sake of efficiency
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