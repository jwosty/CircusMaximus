namespace CircusMaximus.TupleClassExtensions
open System

type Tuple =
  static member private IndErr() = raise (new ArgumentException("The index was greater than the tuple size."))
  
  static member get(index, (a, b)) =
    match index with
    | 0 -> a
    | 1 -> b
    | _ -> Tuple.IndErr()
  
  static member get(index, (a, b, c)) =
    match index with
    | 0 -> a
    | 1 -> b
    | 2 -> c
    | _ -> Tuple.IndErr()
  
  static member get(index, (a, b, c, d)) =
    match index with
    | 0 -> a
    | 1 -> b
    | 2 -> c
    | 3 -> d
    | _ -> Tuple.IndErr()