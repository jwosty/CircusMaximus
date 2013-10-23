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
        let mutable player = new Player.Player(Vector2.Zero, 0.0, 0.0)
        let mutable playerTexture = Unchecked.defaultof<_>
        do
          this.Content.RootDirectory <- "Content"
          graphics.IsFullScreen <- false
     
        /// Overridden from the base Game.Initialize. Once the GraphicsDevice is setup,
        /// we'll use the viewport to initialize some values.
        override this.Initialize() = base.Initialize()

        /// Load your graphics content.
        override this.LoadContent() =
            // Create a new SpriteBatch, which can be use to draw textures.
            playerSpriteBatch <- new SpriteBatch(graphics.GraphicsDevice)
            playerTexture <- Player.loadContent this.Content

        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        override this.Update ( gameTime:GameTime) =
            base.Update (gameTime)

        /// This is called when the game should draw itself. 
        override this.Draw (gameTime:GameTime) =
            graphics.GraphicsDevice.Clear (Color.CornflowerBlue)
            base.Draw (gameTime)
            
            playerSpriteBatch.Begin()
            Player.draw player playerSpriteBatch playerTexture
            playerSpriteBatch.End()