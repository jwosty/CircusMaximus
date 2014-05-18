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
open CircusMaximus
open CircusMaximus.Extensions
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Graphics
open CircusMaximus.Functions

/// Default Project Template
type GameWindow() as this =
  inherit Microsoft.Xna.Framework.Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable playerScreens = Unchecked.defaultof<_>
  let mutable game = Unchecked.defaultof<_>
  // 1st place, 2nd place, etc
  let mutable lastPlacing = 0
  // A general-purpose sprite batch
  let mutable generalBatch = Unchecked.defaultof<_>
  let mutable fontBatch = Unchecked.defaultof<_>
  let mutable assets = Unchecked.defaultof<_>
  
  let mutable lastMouse = Mouse.GetState()
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
    playerScreens <- PlayerScreen.createScreens this.GraphicsDevice Player.numPlayers
    generalBatch <- new SpriteBatch(this.GraphicsDevice)
    fontBatch <- new SpriteBatch(this.GraphicsDevice)
    game <- Game.init (new Random()) this.WindowDimensions
  
  /// Load your graphics content.
  override this.LoadContent() = assets <- loadContent this.Content this.GraphicsDevice Player.numPlayers
  
  /// Updates an XNA sound to make sure it matches the given game sound state. Has side effects.
  member this.UpdateSound(sound, realSound: SoundEffectInstance) =
    match sound with
    | Playing times when realSound.State <> Audio.SoundState.Playing ->
      if times < 1 then
        Stopped
      else
        realSound.Play()
        Playing (times - 1)
    | Looping when realSound.State <> Audio.SoundState.Playing ->
      /// TODO: fix the bug here where the sound doesn't play if it already has in the past (possibly a MonoGame bug)
      realSound.Play()
      Looping
    | Paused when realSound.State <> Audio.SoundState.Paused ->
      realSound.Pause()
      Paused
    | Stopped when realSound.State <> Audio.SoundState.Stopped ->
      realSound.Stop()
      Stopped
    | _ -> sound
  
  /// Allows the game to run logic such as updating the world,
  /// checking for collisions, gathering input, and playing audio.
  override this.Update(gameTime:GameTime) =
    base.Update(gameTime)
    let mouse, keyboard, gamepads = Mouse.GetState(), Keyboard.GetState(), [for i in 0..3 -> GamePad.GetState(enum i)]
    // If Game.next returns a Some, use the contained state as the current state; otherwise, exit the game
    match Game.next game ((lastMouse, mouse), (lastKeyboard, keyboard), (lastGamepads, gamepads)) with
    | Some(newGame, newFields) -> (game <- { game with fields = newFields })
    | None -> this.Exit()
    
    lastMouse <- mouse
    lastKeyboard <- keyboard
    lastGamepads <- gamepads
    
    // Play, pause, or stop sounds if needed
    let sounds =
      { Chariots =
          List.map2
            (fun sound realSound -> this.UpdateSound(sound, realSound))
            game.fields.sounds.Chariots assets.ChariotSound
        CrowdCheer = this.UpdateSound(game.fields.sounds.CrowdCheer, assets.CrowdCheerSound) } 
    game <- { game with fields = { game.fields with sounds = sounds } }
  
  /// This is called when the game should draw itself.
  override this.Draw(gameTime:GameTime) =
    // Since the borders btwn the player screens are merely trimmed edges, they show through to the
    // background and become whatever color the screen is cleared with
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw(gameTime)
    GameGraphics.draw assets generalBatch fontBatch this.WindowCenter this.WindowRect playerScreens game