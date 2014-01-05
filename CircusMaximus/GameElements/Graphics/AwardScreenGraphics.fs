module CircusMaximus.Graphics.AwardScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let draw (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) (awardScreen: AwardScreen) (game: Game) (assets: GameContent) =
  let y = (game.settings.windowDimensions / (2 @@ 2)).Y
  game.playerData |> List.iteri
    (fun i playerData ->
      // Do some quick calculations
      let playerN = "Histrio " + toRoman playerData.number
      let x = vecx game.settings.windowDimensions / (float game.playerData.Length + 0.25) * (float i + 0.625)
      let playerCoinAmtStr, playerCoinsStrCol =
        // TODO: extract this first part out into a function since it will be useful throughout the
        // game -- the Romans couldn't say "0 things", only "no things"!
        match playerData.coinBalance with
        | 0 -> "Nullus", Color.Maroon
        | coins -> toRoman coins, Color.Silver
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
          FlatSpriteFont.drawString
            assets.Font fontBatch playerN (x @@ (y - 25.f)) 3.0f Color.White
            (FlatSpriteFont.Center, FlatSpriteFont.Center)
          FlatSpriteFont.drawString
            assets.Font fontBatch playerCoinsStr (x @@ (y + 25.f)) 2.0f playerCoinsStrCol
            (FlatSpriteFont.Center, FlatSpriteFont.Center))))