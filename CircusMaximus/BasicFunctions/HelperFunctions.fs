module CircusMaximus.HelperFunctions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
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

let positionForward position (direction: float) distance = position + (cos direction * distance @@ sin direction * distance)

let placingColor = function
  | 1 -> Color.Gold
  | 2 -> Color.Silver
  | 3 -> new Color(205, 127, 50)
  | _ -> Color.White

let keyJustReleased (lastKeyboard: KeyboardState, keyboard: KeyboardState) key =
    lastKeyboard.IsKeyDown(key) && keyboard.IsKeyUp(key)
  
let keyJustPressed (lastKeyboard: KeyboardState, keyboard: KeyboardState) key =
  lastKeyboard.IsKeyUp(key) && keyboard.IsKeyDown(key)

let gpButtonJustReleased (lastGamepad: GamePadState, gamepad: GamePadState) button =
  lastGamepad.IsButtonDown(button) && gamepad.IsButtonUp(button)

let gpButtonJustPressed (lastGamepad: GamePadState, gamepad: GamePadState) button =
  lastGamepad.IsButtonUp(button) && gamepad.IsButtonDown(button)

let twice x = x, x