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
        let mutable spriteBatch = Unchecked.defaultof<_>
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
            spriteBatch <- new SpriteBatch(graphics.GraphicsDevice)
            playerTexture <- Player.loadContent this.Content

        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        override this.Update ( gameTime:GameTime) =
            // TODO: Add your update logic here                 
            base.Update (gameTime)

        /// This is called when the game should draw itself. 
        override this.Draw (gameTime:GameTime) =
            // Clear the backbuffer
            graphics.GraphicsDevice.Clear (Color.CornflowerBlue)

            spriteBatch.Begin()

            // draw the logo
            //spriteBatch.Draw (logoTexture, Vector2 (130.f, 200.f), Color.White);
            spriteBatch.End()

            //TODO: Add your drawing code here
            base.Draw (gameTime)
