module CircusMaximus.Graphics.HorseScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let drawBarData (generalBatch: SpriteBatch) (topLeft: Vector2) (bottomRight: Vector2) percent (assets: GameContent) =
  generalBatch.Draw(assets.Pixel, new Rectangle(int topLeft.X, int topLeft.Y, bottomRight.X - topLeft.X |> int, bottomRight.Y - topLeft.Y |> int), Color.Gray)
  let barY = MathHelper.Lerp(bottomRight.Y, topLeft.Y, percent)
  generalBatch.Draw(assets.Pixel, new Rectangle(int topLeft.X + 2, int barY + 2, bottomRight.X - topLeft.X - 4.f |> int, bottomRight.Y - barY - 4.f |> int), Color.White)

let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (game: Game) continueButton assets horses =
  fontBatch.DoWithPointClamp (fun fontBatch ->
  generalBatch.DoBasic (fun generalBatch ->
    ButtonGraphics.draw fontBatch generalBatch continueButton assets
    
    let y = game.settings.windowDimensions.Y / 2.f
    let bgTop = y - (float32 assets.AwardBackground.Height / 2.f)
    let bgBottom = y + (float32 assets.AwardBackground.Height / 2.f)
    let barTop = bgTop + 10.f
    let barBottom = bgBottom - 30.f
    let barSpacing = float32 assets.AwardBackground.Width / 4.0f
    
    let accFactor = 1.0f / float32 Player.baseAcceleration
    let tsFactor = 1.0f / float32 Player.baseTopSpeed
    let tuFactor = 1.0f / float32 Player.baseTurn
    
    horses |> List.iteri (fun i (horse: Horses) ->
      let playerNumber = i + 1
      let x = vecx game.settings.windowDimensions / (float horses.Length + 0.25) * (float (playerNumber - 1) + 0.625) |> float32
      
      let barLeftX = x - (float32 assets.AwardBackground.Width / 2.0f)
      let lx = x - (float32 assets.AwardBackground.Width / 2.f) |> float32
      generalBatch.DrawCentered(assets.AwardBackground, x @@ y, Color.White)
      let bx = barLeftX + (barSpacing * 1.f)
      drawBarData generalBatch (bx - 10.f @@ barTop) ((bx + 10.f) @@ barBottom) (float32 horse.acceleration * accFactor) assets
      let bx = barLeftX + (barSpacing * 2.f)
      drawBarData generalBatch (bx - 10.f @@ barTop) ((bx + 10.f) @@ barBottom) (float32 horse.topSpeed * tsFactor) assets
      let bx = barLeftX + (barSpacing * 3.f)
      drawBarData generalBatch (bx - 10.f @@ barTop) ((bx + 10.f) @@ barBottom) (float32 horse.turn * tuFactor) assets
      )))