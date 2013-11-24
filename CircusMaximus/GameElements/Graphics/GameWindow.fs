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
open CircusMaximus.Game

/// Default Project Template
type GameWindow() as this =
  inherit Microsoft.Xna.Framework.Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable gameState =
    let x = 820.0f
    PreRace(
      CommonRaceData(
        [
          x, 740.0f;
          x, 950.0f;
          x, 1160.0f;
          x, 1370.0f;
          x, 1580.0f;
        ] |> List.map (fun (x, y) -> Player.Moving(new MovingData(new PlayerShape(x@@y, 64.0f, 29.0f, 0.0), 0.0, Racetrack.center, None))),
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
          | MidRace(raceData, _) -> raceData.players.Length
          | PostRace raceData -> raceData.players.Length)
    generalBatch <- new SpriteBatch(this.GraphicsDevice)
    fontBatch <- new SpriteBatch(this.GraphicsDevice)
  
  /// Load your graphics content.
  override this.LoadContent() = assets <- loadContent this.Content this.GraphicsDevice ((baseRaceData gameState).players.Length)
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let keyboard, gamepads = Keyboard.GetState(), [for i in 0..3 -> GamePad.GetState(enum i)]
    match nextGame gameState (lastKeyboard, keyboard) (lastGamepads, gamepads) assets with
      | Some newState -> (gameState <- newState)
      | None -> this.Exit()
    lastKeyboard <- keyboard
    lastGamepads <- gamepads

  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw(gameTime)
    GameGraphics.drawGame this.WindowCenter this.WindowRect playerScreens assets generalBatch fontBatch gameState
