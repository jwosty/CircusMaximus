module CircusMaximus.Extensions
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type Microsoft.Xna.Framework.Graphics.SpriteBatch with
  member this.DrawStringCentered(font: SpriteFont, string: string, position, color) =
    this.DrawString(font, string, font.MeasureString(string) / new Vector2(2.0f, 2.0f), color)