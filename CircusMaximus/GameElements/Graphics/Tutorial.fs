module CircusMaximus.Graphics.TutorialGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types

let draw graphics assets spriteBatch fontBatch settings (tutorial: Tutorial) = WorldGraphics.draw graphics spriteBatch assets settings fontBatch tutorial.players