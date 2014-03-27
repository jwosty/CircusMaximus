module CircusMaximus.Graphics.HorseScreenGraphics
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open CircusMaximus
open CircusMaximus.HelperFunctions
open CircusMaximus.Extensions
open CircusMaximus.Graphics
open CircusMaximus.Types
open CircusMaximus.FlatSpriteFont

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

let drawHorseBarData generalBatch fontBatch barTop barBottom x percent name assets =
  
  drawBarData generalBatch (x - 10.f @@ barTop) (x + 10.f @@ barBottom) percent assets
  FlatSpriteFont.drawString
    assets.Font fontBatch name (x @@ barBottom + 10.f) 1.5f Color.White
    (Center, Center)

let draw (assets: GameContent) (fontBatch: SpriteBatch) (generalBatch: SpriteBatch) horses (buttonGroup: ButtonGroup) (game: Game) =
  fontBatch.DoWithPointClamp (fun fontBatch ->
  generalBatch.DoBasic (fun generalBatch ->
    List.iter
      (fun button -> ButtonGraphics.draw fontBatch generalBatch button assets)
      buttonGroup.buttons
    
    let y = game.settings.windowDimensions.Y / 2.f
    let bgTop = y - (float32 assets.AwardBackground.Height / 2.f)
    let bgBottom = y + (float32 assets.AwardBackground.Height / 2.f)
    let barTop = bgTop + 51.f
    let barBottom = bgBottom - 30.f
    let barSpacing = float32 assets.AwardBackground.Width / 4.0f
    
    let accFactor = 1.0f / float32 Player.baseAcceleration
    let tsFactor = 1.0f / float32 Player.baseTopSpeed
    let tuFactor = 1.0f / float32 Player.baseTurn
    
    horses |> List.iteri (fun i (horse: Horses) ->
      let playerNumber = i + 1
      let x = vecx game.settings.windowDimensions / (float horses.Length + 0.25) * (float (playerNumber - 1) + 0.625) |> float32
      
      let playerColor, playerColorString = Player.colorWithString playerNumber
      
      let barLeftX = x - (float32 assets.AwardBackground.Width / 2.0f)
      let lx = x - (float32 assets.AwardBackground.Width / 2.f) |> float32
      generalBatch.DrawCentered(assets.AwardBackground, x @@ y, Color.White)
      
      // title (player)
      let str = "Auriga " + playerColorString
      FlatSpriteFont.drawString assets.Font fontBatch str (x @@ (bgTop + 17.f)) 2.f playerColor (Center, Center)
      // TODO: check translation
      FlatSpriteFont.drawString assets.Font fontBatch "Informatiae Equo" (x @@ (bgTop + 34.f)) 2.f playerColor (Center, Center)
      
      // "acc." = acceleratio
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 1.f)) (float32 horse.acceleration * accFactor) "acc." assets
      // "vel." = velocitas
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 2.f)) (float32 horse.topSpeed * tsFactor) "vel." assets
      // "spat." = spatium
      drawHorseBarData generalBatch fontBatch barTop barBottom (barLeftX + (barSpacing * 3.f)) (float32 horse.turn * tuFactor) "spat." assets)))