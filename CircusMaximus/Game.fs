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
    ] |> List.map (fun (x, y) -> new Player.Player(new Vector2(x, y), Player.degreesToRadians -180.0, 0.0, 0))
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
    playerScreens <- PlayerScreen.createScreens this.GraphicsDevice
    font <- this.Content.Load<Texture2D>("font")
  
  /// Load your graphics content.
  override this.LoadContent() =
    playerTexture <- Player.loadContent this.Content
    racetrackTextures <- Racetrack.loadContent this.Content
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard = Keyboard.GetState()
    if keyboard.IsKeyDown(Keys.Escape) then this.Exit()
    
    players <- players |>
      List.mapi
        (fun i player ->
          if i = 0 then Player.update (Player.getPowerTurnFromKeyboard keyboard) player
          else Player.update (Player.getPowerTurnFromGamepad (GamePad.GetState(enum <| i - 1))) player)
  
  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw (gameTime)
    List.iter2 (PlayerScreen.drawSingle this.DrawWorld) players playerScreens
  
  member this.DrawWorld((sb, _): PlayerScreen.PlayerScreen) =
    for x in 0..9 do
      for y in 0..2 do
        Racetrack.drawSingle sb racetrackTextures.[x, y] x y
    List.iter (fun player -> Player.draw player sb playerTexture font) players