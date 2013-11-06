namespace CircusMaxmius
open System
open Microsoft.Xna.Framework

type Circle =
  struct
    val public Center: Vector2
    val public Width: float32
    val public Height: float32
    
    new(c, w, h) = { Center = c; Width = w; Height = h }
  end