module CircusMaximus.ItemGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.State

let getItemImage item (assets: GameContent) =
  match item with
  | Item.SugarCubes -> assets.ItemSugarCubes

/// Draws a single item on the screen. Does not call spriteBatch.Begin or spriteBatch.End.
let draw (spriteBatch: SpriteBatch) item position (assets: GameContent) =
  spriteBatch.DrawCentered(getItemImage item assets, position, Color.White)

let defaultItemImageHeight = 32
let defaultItemImageWidth = 32

let defaultItemSelectorImageWidth = 40
let defaultItemSelectorImageHeight = 40