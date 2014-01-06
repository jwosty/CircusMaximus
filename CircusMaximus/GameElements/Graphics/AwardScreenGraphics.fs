module CircusMaximus.Graphics.AwardScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (awardScreen: AwardScreen) (settings: GameSettings) (assets: GameContent) =
  let y = (settings.windowDimensions / (2 @@ 2)).Y
  awardScreen.playerDataAndWinnings |> List.iter
    (fun (playerData, playerWinnings) ->
      // Do some quick calculations
      let playerN = "Histrio " + toRoman playerData.number
      let x = vecx settings.windowDimensions / (float awardScreen.playerDataAndWinnings.Length + 0.25) * (float (playerData.number - 1) + 0.625)
      let playerCoinAmtStr, playerCoinsStrCol =
        // TODO: extract this first part out into a function since it will be useful throughout the
        // game -- the Romans couldn't say "0 things", only "no things"!
        match playerData.coinBalance + playerWinnings with
        | 0 -> "Nullus", Color.Maroon
        | coins -> toRoman coins, Color.Silver
      let playerWinningsStr =
        match playerWinnings with
        | 0 -> ""
        | coins -> "+" + toRoman coins
      let playerCoinsStr = playerCoinAmtStr + " nummi"
      
      // Actually draw everything now
      fontBatch.DoWithPointClamp (fun fontBatch ->
        generalBatch.DoBasic (fun generalBatch ->
          // Draw the buttons
          ButtonGraphics.draw fontBatch generalBatch awardScreen.continueButton assets
          ButtonGraphics.draw fontBatch generalBatch awardScreen.mainMenuButton assets
          
          // Draw the player background image
          generalBatch.DrawCentered(assets.AwardBackground, x @@ y, Color.White)
          // Draw player text
          FlatSpriteFont.drawString     // "heading"
            assets.Font fontBatch playerN (x @@ (y - 25.f)) 3.f Color.White
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString     // current coin balance
            assets.Font fontBatch playerCoinsStr (x @@ (y + 25.f)) 2.f playerCoinsStrCol
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString
            assets.Font fontBatch playerWinningsStr (x @@ (y + 37.f)) 1.f Color.Green
            (FlatSpriteFont.Center, FlatSpriteFont.Center))))