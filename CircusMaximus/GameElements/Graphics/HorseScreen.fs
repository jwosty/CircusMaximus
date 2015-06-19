module CircusMaximus.Graphics.HorseScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.FlatSpriteFont
open CircusMaximus.Functions
open CircusMaximus.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

let drawBarData (generalBatch: SpriteBatch) (topLeft: Vector2) (bottomRight: Vector2) percent (assets: GameContent) =
  let centerColor =
    if percent >= 0.5f then
      let f = percent * -2.f + 2.f
      new Color(f, 1.f, f)
    else
      let f = percent * 2.f
      new Color(1.f, f, f)
  generalBatch.Draw(assets.Pixel, new Rectangle(int topLeft.X, int topLeft.Y, bottomRight.X - topLeft.X |> int, bottomRight.Y - topLeft.Y |> int), Color.Gray)
  let barY = MathHelper.Lerp(bottomRight.Y, topLeft.Y, percent)
  generalBatch.Draw(assets.Pixel, new Rectangle(int topLeft.X + 2, int barY + 2, bottomRight.X - topLeft.X - 4.f |> int, bottomRight.Y - barY - 4.f |> int), centerColor)

let drawHorseBarData generalBatch fontBatch (barTop: float32<px>) barBottom x percent name assets =
  drawBarData generalBatch (xnaVec2 (x - 10.f<px> @@ barTop)) (xnaVec2 (x + 10.f<px> @@ barBottom)) percent assets
  FlatSpriteFont.drawString
    assets.Font fontBatch name (x @@ barBottom + 10.f<px>) 1.5 Color.White
    (Center, Center)

let draw graphics (assets: GameContent) (generalBatch: SpriteBatch) (fontBatch: SpriteBatch) (horseScreen: HorseScreen) (game: Game) =
  fontBatch.DoWithPointClamp (fun fontBatch ->
  generalBatch.DoBasic (fun generalBatch ->
    List.iter
      (fun button -> ButtonGraphics.draw fontBatch generalBatch button assets)
      horseScreen.buttons.buttons
    
    let y = game.fields.settings.windowDimensions.Y / 2.f
    let bgTop = y - (float32 assets.AwardBackground.Height * 1.f<px> / 2.f)
    let bgBottom = y + (float32 assets.AwardBackground.Height * 1.f<px> / 2.f)
    let barTop = bgTop + 51.f<px>
    let barBottom = bgBottom - 30.f<px>
    let barSpacing = float32 assets.AwardBackground.Width * 1.f<px> / 4.0f
    
    let accFactor = 1.0f / float32 Player.baseAcceleration
    let tsFactor = 1.0f / float32 Player.baseTopSpeed
    let tuFactor = 1.0f / float32 Player.baseTurn
    
    horseScreen.horses |> List.iteri (fun i (horse: Horses) ->
      let playerNumber = i + 1
      let x = game.fields.settings.windowDimensions.X / (float32 horseScreen.horses.Length + 0.25f) * (float32 (playerNumber - 1) + 0.625f)
      
      let playerColor, playerColorString = playerColorWithString playerNumber
      
      let barLeftX = x - (float32 assets.AwardBackground.Width * 1.f<px> / 2.0f)
      let lx = x - (float32 assets.AwardBackground.Width * 1.f<px> / 2.f) |> float32
      generalBatch.DrawCentered(assets.AwardBackground, xnaVec2 (x @@ y), Color.White)
      
      // title (player)
      let str = "Auriga " + playerColorString
      FlatSpriteFont.drawString assets.Font fontBatch str (x @@ (bgTop + 17.f<px>)) 2. playerColor (Center, Center)
      // TODO: check translation
      FlatSpriteFont.drawString assets.Font fontBatch "Informatiae Equo" (x @@ (bgTop + 34.f<px>)) 2. playerColor (Center, Center)
      
      // "acc." = acceleratio
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 1.f)) (float32 horse.acceleration * accFactor) "acc." assets
      // "vel." = velocitas
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 2.f)) (float32 horse.topSpeed * tsFactor) "vel." assets
      // "spat." = spatium
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 3.f)) (float32 horse.turn * tuFactor) "spat." assets)))