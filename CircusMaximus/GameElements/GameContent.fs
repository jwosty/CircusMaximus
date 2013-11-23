namespace CircusMaximus
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

type GameContent =
  { Pixel: Texture2D
    RacetrackTextures: Texture2D[,]
    ChariotTexture: Texture2D
    Font: Texture2D

    ChariotSound: SoundEffect
    CrowdCheerSound: SoundEffect }

[<AutoOpen>]
module GameContentFunctions =
  let loadImage img (content: ContentManager) = content.Load<Texture2D>("images/" + img)
  let loadSound snd (content: ContentManager) = content.Load<SoundEffect>("sounds/" + snd)
  
  let loadContent (content: ContentManager) graphicsDevice =
    { Pixel =
        let pt = new Texture2D(graphicsDevice, 1, 1)
        pt.SetData([|Color.White|])
        pt;
      // Use a 2D array because there aren't 2D lists, and a 1D list would be harder to deal with here
      // The GIMP plugin that split the image generates the files in the format y-x.png -- I should fix
      // that sometime
      RacetrackTextures = Array2D.init 10 3 (fun x y -> loadImage (sprintf "racetrack/%i-%i.png" y x) content);
      ChariotTexture = loadImage "chariot" content;
      Font = loadImage "font" content;
      
      ChariotSound = loadSound "chariot" content;
      CrowdCheerSound = loadSound "cheer1" content
    }