module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework

let inline (@@) a b = new Vector2(float32 a, float32 b)

let inline (@~) a b = new Nullable<_>(new Vector2(float32 a, float32 b))

module List =
  let consecutiveDo predicate list =
    let max = List.length list - 1
    for i in 0..max do
      let nextI = if i = max then 0 else i + 1
      predicate (list.[i]) (list.[nextI])