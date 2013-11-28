/// A module to draw game states. It's dirty because it, by nature, has side effects
module CircusMaximus.GameGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.PlayerGraphics
open CircusMaximus.State

let drawHUD fb (assets: GameContent) (player: Player) ((sb, rect): PlayerScreen.PlayerScreen) =
  match player.motionState with
  | Moving velocity ->
    FlatSpriteFont.drawString
      assets.Font fb
      (sprintf "Flexus: %s" (MathHelper.Clamp(player.turns, 0, Int32.MaxValue) |> toRoman))
      (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y)
      3.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Min)
    match player.finishState with
    | Finished placing ->
        let color = match placing with | 1 -> Color.Gold | 2 -> Color.Orange | 3 -> Color.OrangeRed | _ -> Color.Gray
        FlatSpriteFont.drawString
          assets.Font fb
          (sprintf "Locus: %s" (toRoman placing))
          (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y + (24))
          3.0f color (FlatSpriteFont.Center, FlatSpriteFont.Min)
    | Racing -> ()
  | Crashed -> ()

let drawWorld ((sb, rect): PlayerScreen.PlayerScreen) assets fontBatch players mainPlayer =
  for x in 0..9 do
    for y in 0..2 do
      Racetrack.drawSingle sb assets.RacetrackTextures.[x, y] x y
  #if DEBUG
  Racetrack.drawBounds Racetrack.collisionBounds assets.Pixel sb
  #endif
  List.iteri (fun i player -> drawPlayer (sb, rect) player (i = mainPlayer) assets fontBatch) players

let drawScreens playerScreens assets (fontBatch: SpriteBatch) players =
  fontBatch.DoWithPointClamp
    (fun fb ->
      List.iteri2
        (PlayerScreen.drawSingle
          (fun (p, s) -> drawWorld s assets fontBatch players p))
          players playerScreens)

/// Draw a game state
let drawGame windowCenter (windowRect: Rectangle) playerScreens assets (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (race: Race) =
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
  | DynamicRace dynamicRaceState ->
    drawScreens playerScreens assets fontBatch race.players
    match dynamicRaceState with
    | MidRace lastPlacing ->
      fontBatch.DoWithPointClamp
        (fun fb ->
          List.iter2 (drawHUD fb assets) race.players playerScreens
          if race.timer <= Race.midRaceBeginPeriod then
            FlatSpriteFont.drawString
              assets.Font fontBatch "Vaditis!" windowCenter 8.0f Color.ForestGreen
              (FlatSpriteFont.Center, FlatSpriteFont.Center))
    | _ -> ()