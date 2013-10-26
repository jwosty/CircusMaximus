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
  let mutable playerSpriteBatch = Unchecked.defaultof<_>
  let mutable player1 = new Player.Player(new Vector2(400.0f, 50.0f), 0.0, 0.0)
  let mutable player2 = new Player.Player(new Vector2(400.0f, 150.0f), 0.0, 0.0)
  let mutable playerTexture = Unchecked.defaultof<_>
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
  
  /// Load your graphics content.
  override this.LoadContent() =
    // Create a new SpriteBatch, which can be use to draw textures.
    playerSpriteBatch <- new SpriteBatch(graphics.GraphicsDevice)
    playerTexture <- Player.loadContent this.Content
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard = Keyboard.GetState()
    if keyboard.IsKeyDown(Keys.Escape) then this.Exit()
    player1 <- Player.update (Player.getPowerTurnFromKeyboard keyboard) player1
    player2 <- Player.update (Player.getPowerTurnFromGamepad <| GamePad.GetState(PlayerIndex.One)) player2

  /// This is called when the game should draw itself. 
  override this.Draw(gameTime:GameTime) =
    graphics.GraphicsDevice.Clear (Color.CornflowerBlue)
    base.Draw (gameTime)
    
    playerSpriteBatch.Begin()
    Player.draw player1 playerSpriteBatch playerTexture
    Player.draw player2 playerSpriteBatch playerTexture
    playerSpriteBatch.End()