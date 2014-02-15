module CircusMaximus.Graphics.TutorialGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let draw assets fontBatch playerScreens settings (Tutorial.Tutorial(players)) =
  WorldGraphics.drawScreens assets fontBatch playerScreens settings players