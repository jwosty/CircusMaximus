module CircusMaximus.HelperFunctions
open System
open Microsoft.FSharp.Core.LanguagePrimitives
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open CircusMaximus.TupleClassExtensions
open CircusMaximus.Types
open CircusMaximus.Types.UnitNames

/// Cosine of a given number
let inline unitlessCos (value: ^T) = cos value
/// Sine of a given number
let inline unitlessSin (value: ^T) = sin value
/// Inverse tangent of the given number
let inline unitlessAtan (value: ^T) = atan value
/// Inverse tangent of y/x where x and y are specified separately
let inline unitlessAtan2 x y = atan2 x y

let inline f32m (n: ^a) = Float32WithMeasure (float32 n)

type OverloadedOperators() =
  static member _Sin (_, n: float<radian>) = float n |> unitlessSin
  static member _Sin (_, n: float32<radian>) = float32 n |> unitlessSin
  static member _Cos (_, n: float<radian>) = float n |> unitlessCos
  static member _Cos (_, n: float32<radian>) = float32 n |> unitlessCos
  static member _Atan (_, n: float) = n |> unitlessAtan |> FloatWithMeasure<radian>
  static member _Atan (_, n: float32) = n |> unitlessAtan |> Float32WithMeasure<radian>
  static member _Atan2 (_, x: float, y: float) = unitlessAtan2 x y |> FloatWithMeasure<radian>
  static member _Atan2 (_, x: float32, y: float32) = unitlessAtan2 x y |> Float32WithMeasure<radian>
  static member CreateVector2 (_, x: float32<'u>, y: float32<'u>) = new Vector2<'u>(x, y)
  static member CreateVector2 (_, x: float32<'u>, y: float<'u>) = new Vector2<'u>(x, f32m y)
  static member CreateVector2 (_, x: float32<'u>, y: int<'u>) = new Vector2<'u>(x, f32m y)
  static member CreateVector2 (_, x: float<'u>, y: float32<'u>) = new Vector2<'u>(f32m x, y)
  static member CreateVector2 (_, x: float<'u>, y: float<'u>) = new Vector2<'u>(f32m x, f32m y)
  static member CreateVector2 (_, x: float<'u>, y: int<'u>) = new Vector2<'u> (f32m x, f32m y)
  static member CreateVector2 (_, x: int<'u>, y: float32<'u>) = new Vector2<'u>(f32m x, y)
  static member CreateVector2 (_, x: int<'u>, y: float<'u>) = new Vector2<'u>(f32m x, f32m y)
  static member CreateVector2 (_, x: int<'u>, y: int<'u>) = new Vector2<'u>(f32m x, f32m y)

let ops = new OverloadedOperators()

/// Generic 2D Vector operator
let inline (@@) (x: ^a) (y: ^b) : Vector2<'u> = ((^T or ^a or ^b) : (static member CreateVector2 : ^T * ^a * ^b -> Vector2<'u>) (ops, x, y))

#nowarn "64"  // All of the ^T s are constrained to OverloadedOperators, but it doesn't compile if I replace them manually
/// Cosine of a given number, in radians
let inline cos (value: ^a) = ((^T or ^a) : (static member _Cos : ^T * ^a -> _) (ops, value))
/// Sine of a given number, in radians
let inline sin (value: ^a) = ((^T or ^a) : (static member _Sin : ^T * ^a -> _) (ops, value))
/// Inverse tangent of the given number
let inline atan (value: ^a) = ((^T or ^a) : (static member _Atan : ^T * ^a -> _) (ops, value))
/// Inverse tangent of y/x where x and y are specified separately
let inline atan2 (y: ^a) (x: ^b) = ((^T or ^a or ^b) : (static member _Atan2 : ^T * ^a * ^b -> _) (ops, y, x))

let xnaVec2 (v: Vector2<pixel>) = v.xnaVector2

/// Vector2 X position (as a float32/single)
let inline vecxs (vector2: Vector2) = vector2.X
/// Vector2 Y position (as a float32/single)
let inline vecys (vector2: Vector2) = vector2.Y
/// Vector2 X position (as a float/double)
let inline vecx (vector2: Vector2) = float vector2.X
/// Vector2 Y position (as a float/double)
let inline vecy (vector2: Vector2) = float vector2.Y
/// Vector2 cross product
let cross (a: Vector2<_>) (b: Vector2<_>) = (a.X * b.Y) - (a.Y * b.X)
let degreesToRadians d = 2.0 * Math.PI / 360.0 * d

/// Randomly changes two values so that all values still add up to the same total
let unbalanceRandom minUnbalance maxUnbalance (rand: Random) values =
  if maxUnbalance > 0 then
    // The index of the value to increment
    let whichInc = rand.Next(0, List.length values)
    // The index of the value to decrement
    let whichDec =
      let wd = rand.Next(0, (List.length values) - 1)
      if wd >= whichInc then wd + 1
      else wd
    // The amount to unbalance, from 1 to 3
    let amt = rand.Next(minUnbalance, maxUnbalance + 1)
    values |> List.mapi (fun i v ->
      if i = whichInc then v + amt
      elif i = whichDec then v - amt
      else v)
  else values

/// A pipe chain of n length
let rec repeat f x n =
  if n <= 0 then x
  else repeat f (f x) (n - 1)

let ( *@*@* ) a b c = a, b, c

let inline clampMin min value =
  if value < min
  then min
  else value

let inline clampMax max value =
  if value > max
  then max
  else value

let inline clamp min max = clampMin min >> clampMax max

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

let positionForward (position: Vector2<'u>) (direction: float<radian>) (distance: float<'u>) = (cos direction @@ sin direction) * distance + position

let placingColor = function
  | 1 -> Color.Gold
  | 2 -> Color.Silver
  | 3 -> new Color(205, 127, 50)
  | _ -> Color.White

/// Gives a player's color and the human-readable form, given the player's number/index
let playerColorWithString = function
    | 1 -> Color.Red, "ruber"
    | 2 -> Color.Yellow, "fulvus"
    | 3 -> Color.Green, "prasinus"
    | 4 -> Color.Cyan, "querquedulus" // querquedulus = teal, the closest I could find
    | 5 -> Color.Blue, "caeruleus"
    | _ -> Color.White, "albus"

let keyJustReleased (lastKeyboard: KeyboardState, keyboard: KeyboardState) key =
    lastKeyboard.IsKeyDown(key) && keyboard.IsKeyUp(key)
  
let keyJustPressed (lastKeyboard: KeyboardState, keyboard: KeyboardState) key =
  lastKeyboard.IsKeyUp(key) && keyboard.IsKeyDown(key)

let gpButtonJustReleased (lastGamepad: GamePadState, gamepad: GamePadState) button =
  lastGamepad.IsButtonDown(button) && gamepad.IsButtonUp(button)

let gpButtonJustPressed (lastGamepad: GamePadState, gamepad: GamePadState) button =
  lastGamepad.IsButtonUp(button) && gamepad.IsButtonDown(button)

let twice x = x, x