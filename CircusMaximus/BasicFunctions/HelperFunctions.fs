module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework

let inline (@@) a b = new Vector2(float32 a, float32 b)
let inline (@~) a b = new Nullable<_>(new Vector2(float32 a, float32 b))
/// Exclusive 'between' operator
let (><) x (a, b) = (x > a && x < b) || (x < a && x > b)
/// Inclusive 'between' operator
let (>=<) x (a, b) = (x >= a && x <= b) || (x <= a && x >= b)