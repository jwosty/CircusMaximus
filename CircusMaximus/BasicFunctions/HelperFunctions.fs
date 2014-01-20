module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.TupleClassExtensions

/// 2D Vector constructor
let inline (@@) a b = new Vector2(float32 a, float32 b)
/// Nullable 2D constructor
let inline (@~) a b = new Nullable<_>(new Vector2(float32 a, float32 b))
/// Vector2 X position (as a float32/single)
let inline vecxs (vector2: Vector2) = vector2.X
/// Vector2 Y position (as a float32/single)
let inline vecys (vector2: Vector2) = vector2.Y
/// Vector2 X position (as a float/double)
let inline vecx (vector2: Vector2) = float vector2.X
/// Vector2 Y position (as a float/double)
let inline vecy (vector2: Vector2) = float vector2.Y
/// Vector2 cross product
let cross (a: Vector2) (b: Vector2) = (a.X * b.Y) - (a.Y * b.X)
let degreesToRadians d = 2.0 * Math.PI / 360.0 * d

let clampMin min value =
  if value < min
  then min
  else value

let clampMax max value =
  if value > max
  then max
  else value

let clamp value min max = value |> clampMin min |> clampMax max

/// Creates a sort of lazy evaluation by "delaying" the result
let (...<|) f x = fun () -> f x
let (|>...) x f = fun () -> f x

let isSome = function | Some _ -> true | None -> false

/// Exclusive 'between' operator
let (><) x (a, b) = (x > a && x < b) || (x < a && x > b)
/// Inclusive 'between' operator
let (>=<) x (a, b) = (x >= a && x <= b) || (x <= a && x >= b)

/// Converts a number to a Roman Numeral string, courtesy of http://fssnip.net/8h 
let toRoman =
  let numerals = [(1000, "M"); (900, "CM"); (500, "D"); (400, "CD"); (100, "C");
                  (90, "XC"); (50, "L"); (40, "XL"); (10, "X"); (9, "IX");
                  (5, "V"); (4, "IV"); (1, "I")]
  let rec acc (v, r) (m, s) = if (v < m) then (v, r) else acc (v-m, r+s) (m, s)
  fun n ->
    List.fold acc (n, "") numerals |> snd

let positionForward position (direction: float) distance = position + (cos direction * distance @@ sin direction * distance)

let placingColor = function
  | 1 -> Color.Gold
  | 2 -> Color.Silver
  | 3 -> new Color(205, 127, 50)
  | _ -> Color.White

let tup2 x = x, x
let tup3 x = x, x, x
let tup4 x = x, x, x, x
let twice = tup2
let thrice = tup3

module Tuple =
  let t1Init g = g 0
  let t2Init g = g 0, g 1
  let t3Init g = g 0, g 1, g 2
  let t4Init g = g 0, g 1, g 2, g 3
  
  let t1Map p (a) = p a
  let t2Map p (a, b) = p a, p b
  let t3Map p (a, b, c) = p a, p b, p c
  let t4Map p (a, b, c, d) = p a, p b, p c, p d
  
  let t1Map2 p (a) (b) = p a, p b
  let t2Map2 p (a, b) (a', b') = p a a', p b b'
  let t3Map2 p (a, b, c) (a', b', c') = p a a', p b b', p c c'
  let t4Map2 p (a, b, c, d) (a', b', c', d') = p a a', p b b', p c c', p d d'
  
  let t1ConsecutivePairs (a) = a
  let t2ConsecutivePairs (a, b) = (a, b), (b, a)
  let t3ConsecutivePairs (a, b, c) = (a, b), (b, c), (c, a)
  let t4ConsecutivePairs (a, b, c, d) = (a, b), (b, c), (c, d), (d, a)
  
  let t1Iter p (a) = p a
  let t2Iter p (a, b) = p a; p b; ()
  let t3Iter p (a, b, c) = p a; p b; p c; ()
  let t4Iter p (a, b, c, d) = p a; p b; p c; p d; ()
  
  let t1Iter2 p (a) (b) = p a; p b; ()
  let t2Iter2 p (a, b) (a', b') = p a a'; p b b'; ()
  let t3Iter2 p (a, b, c) (a', b', c') = p a a'; p b b'; p c c'; ()
  let t4Iter2 p (a, b, c, d) (a', b', c', d') = p a a'; p b b'; p c c'; p d d'; ()
  
  let t1Zip2 (a) = a
  let t2Zip2 ((a, b), (a', b')) = (a, a'), (b, b')
  let t3Zip2 ((a, b, c), (a', b', c')) = (a, a'), (b, b'), (c, c')
  let t4Zip2 ((a, b, c, d), (a', b', c', d')) = (a, a'), (b, b'), (c, c'), (d, d')
  
  // The definitions are actually the same, but only because we're working with pure tuple (no tuples + lists)
  let t1Unzip2 = t1Zip2
  let t2Unzip2 = t2Zip2
  let t3Unzip2 ((a, a'), (b, b'), (c, c')) = (a, b, c), (a', b', c')
  let t4Unzip2 ((a, a'), (b, b'), (c, c'), (d, d')) = (a, b, c, d), (a', b', c', d')
  
  let t1Reduce predicate a = predicate a
  let t2Reduce predicate (a, b) = predicate a b
  let t3Reduce predicate (a, b, c) = predicate (predicate a b) c
  let t4Reduce predicate (a, b, c, d) = predicate (predicate (predicate a b) c) d
  
  let t2Combine predicate (pairs: (_ * _) list) =
    List.init
      2
      (fun i ->
        pairs
        |> List.map (fun tup -> Tuple.get(i, tup))
        |> List.reduce predicate)
  
  let t3Combine predicate (tuples: (_ * _ * _) list) =
    List.init
      3
      (fun i ->
        tuples
        |> List.map (fun tup -> Tuple.get(i, tup))
        |> List.reduce predicate)
  
  let t4Combine predicate (tuples: (_ * _ * _ * _) list) =
    List.init
      4
      (fun i ->
        tuples
        |> List.map (fun tup -> Tuple.get(i, tup))
        |> List.reduce predicate)