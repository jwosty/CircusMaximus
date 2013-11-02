module CircusMaximus.Extensions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus.HelperFunctions

type Microsoft.Xna.Framework.Graphics.SpriteBatch with
  member this.DrawStringCentered(font: SpriteFont, string: string, position, color) =
    this.DrawString(font, string, font.MeasureString(string) / (2 @@ 2), color)