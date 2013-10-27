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

/// Default Project Template
type CircusMaximusGame() as this =
  inherit Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable globalSpriteBatch = Unchecked.defaultof<_>
  let mutable players =
    [
      400.0f, 50.0f;
      400.0f, 150.0f;
      400.0f, 250.0f;
      400.0f, 350.0f;
      400.0f, 450.0f;
    ] |> List.map (fun (x, y) -> new Player.Player(new Vector2(x, y), 0.0, 0.0))
  let mutable playerTexture = Unchecked.defaultof<_>
  let mutable racetrackTexture = Unchecked.defaultof<_>
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
  
  /// Load your graphics content.
  override this.LoadContent() =
    // Create a new SpriteBatch, which can be use to draw textures.
    globalSpriteBatch <- new SpriteBatch(graphics.GraphicsDevice)
    playerTexture <- Player.loadContent this.Content
    racetrackTexture <- this.Content.Load<Texture2D>("racetrack/0-0")
  
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
  
  member this.DrawWorld((sb, rect): PlayerScreen.PlayerScreen) =
    sb.Draw(racetrackTexture, Vector2.Zero, Color.White)
    List.iter (fun player -> Player.draw player sb playerTexture) players
  
  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    graphics.GraphicsDevice.Clear (Color.CornflowerBlue)
    base.Draw (gameTime)
    List.iter2 (PlayerScreen.screenDraw this.DrawWorld) players playerScreens