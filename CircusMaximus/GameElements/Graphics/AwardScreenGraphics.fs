module CircusMaximus.Graphics.AwardScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (assets: GameContent) (game: Game) =
  let y = (game.settings.windowDimensions / (2 @@ 2)).Y
  game.playerData |> List.iteri
    (fun i playerData ->
      fontBatch.DoWithPointClamp
        (fun fontBatch ->
          let x = vecx game.settings.windowDimensions / (float game.playerData.Length + 0.25) * (float i + 0.625)
          let playerN = "Histrio " + toRoman playerData.number
          let playerCoinAmtStr, playerCoinsStrCol =
            // TODO: extract this first part out into a function since it will be useful throughout the
            // game -- the Romans couldn't say "0 things", only "no things"!
            match playerData.coinBalance with
            | 0 -> "Nullus", Color.Maroon
            | coins -> toRoman coins, Color.Silver
          let playerCoinsStr = playerCoinAmtStr + " nummi"
          // Draw the background image
          generalBatch.Begin()
          generalBatch.DrawCentered(assets.AwardBackground, x @@ y, Color.White)
          generalBatch.End()
          // Draw the text
          FlatSpriteFont.drawString
            assets.Font fontBatch playerN (x @@ (y - 25.f)) 3.0f Color.White
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString
            assets.Font fontBatch playerCoinsStr (x @@ (y + 25.f)) 2.0f playerCoinsStrCol
            (FlatSpriteFont.Center, FlatSpriteFont.Center)))