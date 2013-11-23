namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Microsoft.Xna.Framework.Storage
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Media
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.State.Game
open CircusMaximus.State.Player

/// Default Project Template
type CircusMaximusGame() as this =
  inherit Microsoft.Xna.Framework.Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable gameState =
    let x = 820.0f
    PreRace(
      PreRaceData(
        [
          x, 740.0f;
          x, 950.0f;
          x, 1160.0f;
          x, 1370.0f;
          x, 1580.0f;
        ] |> List.map (fun (x, y) -> Player.Moving(new MovingData(new OrientedRectangle(x@@y, 64.0f, 29.0f, 0.0), 0.0, Racetrack.center, None))),
        0))
  // 1st place, 2nd place, etc
  let mutable lastPlacing = 0
  // A general-purpose sprite batch
  let mutable generalBatch = Unchecked.defaultof<_>
  let mutable fontBatch = Unchecked.defaultof<_>
  let mutable assets = Unchecked.defaultof<_>
  
  let mutable lastKeyboard = Keyboard.GetState()
  let mutable lastGamepads = [for i in 0..3 -> GamePad.GetState(enum i)]
  
  do
    this.Content.RootDirectory <- "Content"
#if DEBUG
    graphics.IsFullScreen <- false
#else
    graphics.IsFullScreen <- true
#endif
    graphics.PreferredBackBufferWidth <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
    graphics.PreferredBackBufferHeight <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
  
  member this.WindowRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight)
  member this.WindowDimensions = graphics.PreferredBackBufferWidth @@ graphics.PreferredBackBufferHeight
  member this.WindowCenter = this.WindowDimensions * (0.5 @@ 0.5)
  
  /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
  /// we'll use the viewport to initialize some values.
  override this.Initialize() =
    base.Initialize()
    this.IsMouseVisible <- true
    playerScreens <-
      PlayerScreen.createScreens
        this.GraphicsDevice
        (match gameState with
          | PreRace raceData -> raceData.players.Length
          | MidRace raceData -> raceData.players.Length
          | PostRace raceData -> raceData.players.Length)
    generalBatch <- new SpriteBatch(this.GraphicsDevice)
    fontBatch <- new SpriteBatch(this.GraphicsDevice)
  
  /// Load your graphics content.
  override this.LoadContent() = assets <- loadContent this.Content this.GraphicsDevice (State.Game.playerQuantity gameState)
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard, gamepads = Keyboard.GetState(), [for i in 0..3 -> GamePad.GetState(enum i)]
    match State.Game.update gameState (lastKeyboard, keyboard) (lastGamepads, gamepads) assets with
      | Some newState -> (gameState <- newState)
      | None -> this.Exit()
    lastKeyboard <- keyboard
    lastGamepads <- gamepads

  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw (gameTime)
    match gameState with
    | PreRace raceData ->
      this.DrawScreens(raceData.players)
      // Draw a dark overlay to indicate that the game hasn't started yet
      generalBatch.Begin()
      generalBatch.Draw(assets.Pixel, this.WindowRect, new Color(Color.Black, 192))
      generalBatch.End()
      // Draw a countdown
      this.FontBatchDo fontBatch
        (fun (fb: SpriteBatch) ->
          FlatSpriteFont.drawString
            assets.Font fontBatch (State.Game.preRaceMaxCount - (raceData.timer / State.Game.preRaceTicksPerCount) |> toRoman)
            this.WindowCenter 8.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Center))
    | MidRace raceData ->
      this.DrawScreens(raceData.players)
      this.FontBatchDo fontBatch
        (fun fb ->
          List.iter2 (this.DrawHUD fb) raceData.players playerScreens
          if raceData.timer <= State.Game.midRaceBeginPeriod then
            FlatSpriteFont.drawString
              assets.Font fontBatch "Vaditis!" this.WindowCenter 8.0f Color.ForestGreen
              (FlatSpriteFont.Center, FlatSpriteFont.Center))
    | PostRace raceData ->
      this.DrawScreens(raceData.players)
  
  member this.DrawScreens(players) =
    this.FontBatchDo fontBatch
      (fun fb ->
        List.iteri2
          (PlayerScreen.drawSingle
            (fun (a, b) -> this.DrawWorld(players, a, b)))
            players playerScreens)
  
  member this.FontBatchDo (fb: SpriteBatch) predicate =
    // SamplerState.PointClamp disables anti-aliasing, which just looks horrible on scaled bitmap fonts
    fb.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null)
    predicate fb |> ignore
    fb.End()
  
  member this.DrawWorld(players, mainPlayer, ((sb, rect): PlayerScreen.PlayerScreen)) =
    for x in 0..9 do
      for y in 0..2 do
        Racetrack.drawSingle sb assets.RacetrackTextures.[x, y] x y
#if DEBUG
    Racetrack.drawBounds Racetrack.collisionBounds assets.Pixel sb
#endif
    List.iteri (fun i player -> Player.draw (sb, rect) player (i = mainPlayer) assets fontBatch) players
  
  member this.DrawHUD fb player ((sb, rect): PlayerScreen.PlayerScreen) =
    match player with
    | Player.Moving player ->
        FlatSpriteFont.drawString
          assets.Font fb
          (sprintf "Flexus: %s" (MathHelper.Clamp(player.turns, 0, Int32.MaxValue) |> toRoman))
          (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y)
          3.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Min)
        match player.placing with
        | Some placing ->
            let color = match placing with | 1 -> Color.Gold | 2 -> Color.Orange | 3 -> Color.OrangeRed | _ -> Color.Gray
            FlatSpriteFont.drawString
              assets.Font fb
              (sprintf "Locus: %s" (toRoman placing))
              (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y + (24))
              3.0f color (FlatSpriteFont.Center, FlatSpriteFont.Min)
        | None -> ()
    | _ -> ()