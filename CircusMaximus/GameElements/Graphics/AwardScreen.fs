module CircusMaximus.Graphics.AwardScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

let draw graphics (assets: GameContent) (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (settings: GameSettings) (awardScreen: AwardScreen) =
  let y = (settings.windowDimensions / (2 @@ 2)).Y
  awardScreen.playerDataAndWinnings |> List.iter
    (fun (playerData, playerWinnings) ->
      // Do some quick calculations
      let playerN = "Auriga " + toRoman playerData.number
      let x = settings.windowDimensions.X / (float32 awardScreen.playerDataAndWinnings.Length + 0.25f) * (float32 (playerData.number - 1) + 0.625f)
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
          List.iter
            (fun button -> ButtonGraphics.draw fontBatch generalBatch button assets)
            awardScreen.buttonGroup.buttons
          
          // Draw the player background image
          generalBatch.DrawCentered(assets.AwardBackground, xnaVec2 (x @@ y), Color.White)
          // Draw player text
          FlatSpriteFont.drawString     // "heading"
            assets.Font fontBatch playerN (x @@ (y - 25.f<px>)) 3. Color.White
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString     // current coin balance
            assets.Font fontBatch playerCoinsStr (x @@ (y + 25.f<px>)) 2. playerCoinsStrCol
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString
            assets.Font fontBatch playerWinningsStr (x @@ (y + 37.f<px>)) 1. Color.Green
            (FlatSpriteFont.Center, FlatSpriteFont.Center))))