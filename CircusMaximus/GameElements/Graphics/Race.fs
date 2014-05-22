module CircusMaximus.Graphics.RaceGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types
open CircusMaximus.Functions

let draw assets (generalBatch: SpriteBatch) fontBatch windowCenter (windowRect: Rectangle) playerScreens settings (race: Race) =
  match race.raceState with
    | PreRace ->
      WorldGraphics.drawScreens assets fontBatch playerScreens settings race.players
      // Draw a dark overlay to indicate that the game hasn't started yet
      generalBatch.DoBasic (fun generalBatch -> generalBatch.Draw(assets.Pixel, windowRect, new Color(Color.Black, 192)))
      // Draw a countdown
      fontBatch.DoWithPointClamp
        (fun (fb: SpriteBatch) ->
          FlatSpriteFont.drawString
            assets.Font fontBatch (Race.preRaceMaxCount - (race.timer / Race.preRaceTicksPerCount) |> toRoman)
            windowCenter 8.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Center))
    
    | _ ->
      WorldGraphics.drawScreens assets fontBatch playerScreens settings race.players
      match race.raceState with
      | MidRace lastPlacing ->
        fontBatch.DoWithPointClamp
          (fun fb ->
            List.iter2 (HUDGraphics.draw generalBatch fb assets) race.players playerScreens
            if race.timer <= Race.midRaceBeginPeriod then
              FlatSpriteFont.drawString
                assets.Font fontBatch "Vaditis!" windowCenter 8.0f Color.ForestGreen
                (FlatSpriteFont.Center, FlatSpriteFont.Center))
      | PostRace(buttonGroup) ->
        fontBatch.DoWithPointClamp
          (fun fontBatch ->
            generalBatch.DoBasic
              (fun generalBatch ->
                PlacingOverlayGraphics.drawOverlay generalBatch fontBatch (windowRect.Width, windowRect.Height) assets race.players
                List.iter
                  (fun button -> ButtonGraphics.draw fontBatch generalBatch button assets)
                  buttonGroup.buttons))
      | PreRace -> ()     // we've already matched for this