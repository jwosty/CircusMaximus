module CircusMaximus.Graphics.ScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let draw screen (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) assets =
  match screen with
  | MainMenu playButton -> fontBatch.DoWithPointClamp (fun fb -> generalBatch.DoBasic (fun gb -> ButtonGraphics.draw fb gb playButton assets))