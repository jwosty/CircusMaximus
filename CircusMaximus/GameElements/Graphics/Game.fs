/// A module to draw game states. It's dirty because it, by nature, has side effects
module CircusMaximus.Graphics.GameGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.State

let drawWorld ((sb, rect): PlayerScreen.PlayerScreen) assets (fontBatch: SpriteBatch) players mainPlayer =
  for x in 0..9 do
    for y in 0..2 do
      Racetrack.drawSingle sb assets.RacetrackTextures.[x, y] x y
  #if DEBUG
  Racetrack.drawBounds Racetrack.collisionBounds assets.Pixel sb
  #endif
  List.iteri
    (fun i player -> PlayerGraphics.drawPlayer (sb, rect) player (i = mainPlayer) assets fontBatch)
    players

let drawScreens playerScreens assets (fontBatch: SpriteBatch) players =
  List.iteri2
    (PlayerScreen.drawSingle (fun (p, s) -> drawWorld s assets fontBatch players p) fontBatch)
    players playerScreens

/// Draw a game state
let drawGame windowCenter (windowRect: Rectangle) playerScreens assets (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (game: Game) =
  match game.gameState with
  | MainMenu mainMenu ->
    MainMenuGraphics.draw mainMenu fontBatch generalBatch assets
  
  | HorseScreen(horses, continueButton) -> HorseScreenGraphics.draw fontBatch generalBatch game continueButton assets horses
  
  | Race race ->
    match race.raceState with
    | PreRace ->
      drawScreens playerScreens assets fontBatch race.players
      // Draw a dark overlay to indicate that the game hasn't started yet
      generalBatch.Begin()
      generalBatch.Draw(assets.Pixel, windowRect, new Color(Color.Black, 192))
      generalBatch.End()
      // Draw a countdown
      fontBatch.DoWithPointClamp
        (fun (fb: SpriteBatch) ->
          FlatSpriteFont.drawString
            assets.Font fontBatch (Race.preRaceMaxCount - (race.timer / Race.preRaceTicksPerCount) |> toRoman)
            windowCenter 8.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Center))
    
    | _ ->
      drawScreens playerScreens assets fontBatch race.players
      match race.raceState with
      | MidRace lastPlacing ->
        fontBatch.DoWithPointClamp
          (fun fb ->
            List.iter2 (HUDGraphics.draw generalBatch fb assets) race.players playerScreens
            if race.timer <= Race.midRaceBeginPeriod then
              FlatSpriteFont.drawString
                assets.Font fontBatch "Vaditis!" windowCenter 8.0f Color.ForestGreen
                (FlatSpriteFont.Center, FlatSpriteFont.Center))
      | PostRace(continueButton) ->
        fontBatch.DoWithPointClamp
          (fun fontBatch ->
            generalBatch.DoBasic
              (fun generalBatch ->
                PlacingOverlayGraphics.drawOverlay generalBatch fontBatch (windowRect.Width, windowRect.Height) assets race.players
                ButtonGraphics.draw fontBatch generalBatch continueButton assets))
  
  | AwardScreen awardScreen ->
    AwardScreenGraphics.draw fontBatch generalBatch awardScreen game.settings assets