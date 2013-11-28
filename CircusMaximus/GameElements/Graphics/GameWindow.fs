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
open CircusMaximus.Player
open CircusMaximus.Race

/// Default Project Template
type GameWindow() as this =
  inherit Microsoft.Xna.Framework.Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable raceState = Race.init ()
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
    playerScreens <- PlayerScreen.createScreens this.GraphicsDevice raceState.players.Length
    generalBatch <- new SpriteBatch(this.GraphicsDevice)
    fontBatch <- new SpriteBatch(this.GraphicsDevice)
  
  /// Load your graphics content.
  override this.LoadContent() = assets <- loadContent this.Content this.GraphicsDevice (raceState.players.Length)
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard, gamepads = Keyboard.GetState(), [for i in 0..3 -> GamePad.GetState(enum i)]
    match Race.next raceState (lastKeyboard, keyboard) (lastGamepads, gamepads) assets with
      | Some newState -> (raceState <- newState)
      | None -> this.Exit()
    lastKeyboard <- keyboard
    lastGamepads <- gamepads

  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw(gameTime)
    GameGraphics.drawGame this.WindowCenter this.WindowRect playerScreens assets generalBatch fontBatch raceState