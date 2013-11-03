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
open Extensions
open HelperFunctions
open BoundingBox2D

/// Default Project Template
type CircusMaximusGame() as this =
  inherit Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable players =
    let x = 820.0f
    [
      x, 740.0f;
      x, 950.0f;
      x, 1160.0f;
      x, 1370.0f;
      x, 1580.0f;
    ] |> List.map (fun (x, y) -> new Player.Player(new BoundingBox2D(x@@y, 0.0, 64.0f, 29.0f), 0.0, Racetrack.center))
  let mutable fontBatch = Unchecked.defaultof<_>
  let mutable pixelTexture = Unchecked.defaultof<_>
  let mutable playerTexture = Unchecked.defaultof<_>
  let mutable racetrackTextures = Unchecked.defaultof<_>
  let mutable font = Unchecked.defaultof<_>
  do
    this.Content.RootDirectory <- "Content"
#if DEBUG
    graphics.IsFullScreen <- false
#else
    graphics.IsFullScreen <- true
#endif
    graphics.PreferredBackBufferWidth <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
    graphics.PreferredBackBufferHeight <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
  
  /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
  /// we'll use the viewport to initialize some values.
  override this.Initialize() =
    base.Initialize()
    this.IsMouseVisible <- true
    playerScreens <- PlayerScreen.createScreens this.GraphicsDevice players.Length
    fontBatch <- new SpriteBatch(this.GraphicsDevice)
  
  /// Load your graphics content.
  override this.LoadContent() =
    pixelTexture <- Extensions.loadContent this.GraphicsDevice
    playerTexture <- Player.loadContent this.Content
    racetrackTextures <- Racetrack.loadContent this.Content
    font <- this.Content.Load<Texture2D>("font")
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard = Keyboard.GetState()
    if keyboard.IsKeyDown(Keys.Escape) then this.Exit()
    
    players <- players |>
      List.mapi
        (fun i player ->
          let otherPlayers = List.removeIndex i players
          if i = 0 then Player.update (Player.getPowerTurnFromKeyboard keyboard) otherPlayers player (keyboard.IsKeyDown(Keys.Q)) Racetrack.center
          else
            let gamepad = GamePad.GetState(enum <| i - 1)
            Player.update (Player.getPowerTurnFromGamepad gamepad) otherPlayers player (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center)
  
  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw (gameTime)
    // SamplerState.PointClamp disables anti-aliasing, which just looks horrible on scaled bitmap fonts
    fontBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null)
    List.iteri2 (PlayerScreen.drawSingle this.DrawWorld) players playerScreens
    fontBatch.End()
    fontBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null)
    List.iter2 this.DrawHUD players playerScreens
    fontBatch.End()
  
  member this.DrawWorld(mainPlayer, ((sb, rect): PlayerScreen.PlayerScreen)) =
    for x in 0..9 do
      for y in 0..2 do
        Racetrack.drawSingle sb racetrackTextures.[x, y] x y
    List.iteri (fun i player -> Player.draw (sb, rect) player (i = mainPlayer) playerTexture font fontBatch pixelTexture) players
  
  member this.DrawHUD player ((sb, rect): PlayerScreen.PlayerScreen) =
    FlatSpriteFont.drawString
      font fontBatch
      (sprintf "Turns: %i" (MathHelper.Clamp(player.turns, 0, Int32.MaxValue)))
      (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y)
      3.0f Color.White
      (FlatSpriteFont.Center, FlatSpriteFont.Min)
    FlatSpriteFont.drawString
      font fontBatch
      (sprintf "LastTurnLeft: %b" player.lastTurnedLeft)
      (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y + 24)
      3.0f Color.White
      (FlatSpriteFont.Center, FlatSpriteFont.Min)