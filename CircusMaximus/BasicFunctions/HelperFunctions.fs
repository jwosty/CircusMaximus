module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework

let inline (@@) a b = new Vector2(float32 a, float32 b)
let inline (@~) a b = new Nullable<_>(new Vector2(float32 a, float32 b))
// Vector2 cross product
let cross (a: Vector2) (b: Vector2) = (a.X * b.Y) - (a.Y * b.X)

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

module Tuple =
  let twice x = x, x
  
  let t2Init g = g 0, g 1
  let t3Init g = g 0, g 1, g 2
  let t4Init g = g 0, g 1, g 2, g 3
  
  let t2Map p (a, b) = p a, p b
  let t3Map p (a, b, c) = p a, p b, p c
  let t4Map p (a, b, c, d) = p a, p b, p c, p d
  
  let t2ConsecutivePairs (a, b) = (a, b), (b, a)
  let t3ConsecutivePairs (a, b, c) = (a, b), (b, c), (c, a)
  let t4ConsecutivePairs (a, b, c, d) = (a, b), (b, c), (c, d), (d, a)
  
  let t2Iter p (a, b) = p a; p b; ()
  let t3Iter p (a, b, c) = p a; p b; p c; ()
  let t4Iter p (a, b, c, d) = p a; p b; p c; p d; ()
  
  let t2Iter2 p (a, b) (a', b') = p a a'; p b b'; ()
  let t3Iter2 p (a, b, c) (a', b', c') = p a a'; p b b'; p c c'
  let t4Iter2 p (a, b, c, d) (a', b', c', d') = p a a'; p b b'; p c c'; p d d'