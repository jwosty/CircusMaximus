module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework

let inline (@@) a b = new Vector2(float32 a, float32 b)

let inline (@~) a b = new Nullable<_>(new Vector2(float32 a, float32 b))

module List =
  /// Returns the consecutive pairs of a list (including the first and last elements together)
  let consecutivePairs list =
    let max = List.length list - 1
    List.mapi
      (fun i _ ->
        let nextI = if i = max then 0 else i + 1
        list.[i], list.[nextI])
      list