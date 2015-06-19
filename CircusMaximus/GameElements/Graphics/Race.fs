module CircusMaximus.Graphics.RaceGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.Functions
open CircusMaximus.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

let draw (graphics: GraphicsDeviceManager) assets spriteBatch fontBatch (windowDimensions: Vector2<px>) settings (race: Race) =
  let windowCenter = (graphics.PreferredBackBufferWidth * 1<px> @@ graphics.PreferredBackBufferHeight * 1<px>) / 2
  match race.raceState with
    | PreRace ->
      WorldGraphics.draw graphics spriteBatch assets settings fontBatch race.players
      // Draw a dark overlay to indicate that the game hasn't started yet
      spriteBatch.DoBasic (fun spriteBatch -> spriteBatch.Draw(assets.Pixel, new Rectangle(0, 0, int windowDimensions.X, int windowDimensions.Y), new Color(Color.Black, 192)))
      // Draw a countdown
      fontBatch.DoWithPointClamp
        (fun (fb: SpriteBatch) ->
          FlatSpriteFont.drawString
            assets.Font fontBatch (Race.preRaceMaxCount - (race.elapsedTime / Race.preRaceTicksPerCount) |> int |> toRoman)
            windowCenter 8.0 Color.White (FlatSpriteFont.Center, FlatSpriteFont.Center))
    
    | _ ->
      WorldGraphics.draw graphics spriteBatch assets settings fontBatch race.players
      match race.raceState with
      | MidRace lastPlacing ->
        fontBatch.DoWithPointClamp
          (fun fb ->
            //List.iter2 (HUDGraphics.draw spriteBatch fb assets) race.players playerScreens
            if race.elapsedTime <= Race.midRaceBeginPeriod then
              FlatSpriteFont.drawString
                assets.Font fontBatch "Vaditis!" windowCenter 8.0 Color.ForestGreen
                (FlatSpriteFont.Center, FlatSpriteFont.Center))
      | PostRace(buttonGroup) ->
        fontBatch.DoWithPointClamp
          (fun fontBatch ->
            spriteBatch.DoBasic
              (fun generalBatch ->
                PlacingOverlayGraphics.drawOverlay generalBatch fontBatch windowDimensions assets race.players
                List.iter
                  (fun button -> ButtonGraphics.draw fontBatch generalBatch button assets)
                  buttonGroup.buttons))
      | PreRace -> ()     // we've already matched for this