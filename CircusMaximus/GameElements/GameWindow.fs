namespace CircusMaximus
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
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
open CircusMaximus.Functions
open CircusMaximus.Graphics
open CircusMaximus.HelperFunctions
open CircusMaximus.Types
open CircusMaximus.Types.UnitSymbols

/// Default Project Template
type GameWindow() as this =
  inherit Microsoft.Xna.Framework.Game()
  let graphics = new GraphicsDeviceManager(this)
  let mutable game = Unchecked.defaultof<_>
  // 1st place, 2nd place, etc
  let mutable lastPlacing = 0
  // A general-purpose sprite batch
  let mutable generalBatch = Unchecked.defaultof<_>
  let mutable fontBatch = Unchecked.defaultof<_>
  let mutable assets = Unchecked.defaultof<_>
  let mutable fpsTimer = 0.<s>
  let rand = new Random()
  
  let mutable input = GameInput.initInitial <| Keyboard.GetState () <| new MouseInput(Mouse.GetState ()) <| [for i in 0..3 -> GamePad.GetState(enum i)]
  
  do
    this.Content.RootDirectory <- "../Resources/Content"
#if DEBUG
    graphics.IsFullScreen <- false
#else
    graphics.IsFullScreen <- true
#endif
    graphics.PreferredBackBufferWidth <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
    graphics.PreferredBackBufferHeight <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
    this.IsFixedTimeStep <- false
  
  member this.WindowDimensions = graphics.PreferredBackBufferWidth * 1<px> @@ graphics.PreferredBackBufferHeight * 1<px>
  member this.WindowCenter = this.WindowDimensions * (0.5 @@ 0.5)
  
  /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
  /// we'll use the viewport to initialize some values.
  override this.Initialize() =
    base.Initialize()
    Game.initFunctions ()
    this.IsMouseVisible <- false
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
    base.Update gameTime
    // Fetch input
    input <- input.shift <| Keyboard.GetState () <| new MouseInput(Mouse.GetState ()) <| [for i in 0..3 -> GamePad.GetState <| enum i]
    let deltaTime = gameTime.ElapsedGameTime.TotalSeconds * 1.<s>
    fpsTimer <- fpsTimer + deltaTime
    // If Game.next returns a Some, use the contained state as the current state; otherwise, exit the game
    match Game.next game deltaTime input with
    | Some(newScreen, newFields) -> (game <- { game with gameScreen = newScreen; fields = newFields })
    | None -> this.Exit()
    
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
    graphics.GraphicsDevice.Clear (Color.Black)
    base.Draw(gameTime)
    let deltaTime = gameTime.ElapsedGameTime.TotalSeconds * 1.<s>
    GameGraphics.draw graphics assets generalBatch fontBatch this.WindowCenter this.WindowDimensions game
    if fpsTimer >= 0.5<s> then
      this.Window.Title <- sprintf "CircusMaximus (%i FPS)" (int (1.<fr> / deltaTime))
      fpsTimer <- 0.<s> 