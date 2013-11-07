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
    ] |> List.map (fun (x, y) -> Player.Moving(new State.Player.Moving(new OrientedRectangle(x@@y, 64.0f, 29.0f, 0.0), 0.0, Racetrack.center, None)))
  // 1st place, 2nd place, etc
  let mutable lastPlacing = 0
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
    
    let collisions =
      (Racetrack.collisionBounds
        :: (players |> List.map Player.playerBB))
        |> Collision.collideWorld
    
    players <-
      List.mapi2
        (fun i player collisionResult ->
          let collision = match collisionResult with | Collision.Result_Poly(lines) -> lines | _ -> failwith "Bad player collision result; that's not supposed to happen... It's probably a bug!"
          let otherPlayers = List.removeIndex i players
          let player, p =
            if i = 0 then Player.update (Player.getPowerTurnFromKeyboard keyboard) player collision lastPlacing (keyboard.IsKeyDown(Keys.Q)) Racetrack.center
            else
              let gamepad = GamePad.GetState(enum <| i - 1)
              Player.update (Player.getPowerTurnFromGamepad gamepad) player collision lastPlacing (gamepad.Buttons.A = ButtonState.Pressed) Racetrack.center
          match p with | Some placing -> (lastPlacing <- placing) | None -> ()
          player)
        players
        (collisions |> List.tail)
  
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
#if DEBUG
    Racetrack.drawBounds Racetrack.collisionBounds pixelTexture sb
#endif
    List.iteri (fun i player -> Player.draw (sb, rect) player (i = mainPlayer) playerTexture font fontBatch pixelTexture) players
  
  member this.DrawHUD player ((sb, rect): PlayerScreen.PlayerScreen) =
    match player with
    | Player.Moving player ->
        FlatSpriteFont.drawString
          font fontBatch
          (sprintf "Flexus: %s" (MathHelper.Clamp(player.turns, 0, Int32.MaxValue) |> toRoman))
          (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y)
          3.0f Color.White (FlatSpriteFont.Center, FlatSpriteFont.Min)
        match player.placing with
        | Some placing ->
            let color = match placing with | 1 -> Color.Gold | 2 -> Color.Orange | 3 -> Color.OrangeRed | _ -> Color.Gray
            FlatSpriteFont.drawString
              font fontBatch
              (sprintf "Locus: %s" (toRoman placing))
              (float32 rect.X + (float32 rect.Width / 2.0f) @@ rect.Y + (24))
              3.0f color (FlatSpriteFont.Center, FlatSpriteFont.Min)
        | None -> ()
    | _ -> ()