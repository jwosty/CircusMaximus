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
    PlacingBackground: Texture2D
    Particle: Texture2D
    Button: Texture2D
    
    ChariotSound: SoundEffectInstance list
    CrowdCheerSound: SoundEffectInstance }

[<AutoOpen>]
module GameContentFunctions =
  let loadImage (content: ContentManager) img = content.Load<Texture2D>("images/" + img)
  let loadSound (content: ContentManager) snd = content.Load<SoundEffect>("sounds/" + snd)
  let loadSoundInstance content snd = (loadSound content snd).CreateInstance()
  
  let loadContent (content: ContentManager) graphicsDevice playerQuantity =
    let loadImage, loadSound, loadSoundInstance = loadImage content, loadSound content, loadSoundInstance content
    { Pixel =
        let pt = new Texture2D(graphicsDevice, 1, 1)
        pt.SetData([|Color.White|])
        pt;
      // Use a 2D array because there aren't 2D lists, and a 1D list would be harder to deal with here
      // The GIMP plugin that split the image generates the files in the format y-x.png -- I should fix
      // that sometime
      RacetrackTextures = Array2D.init 10 3 (fun x y -> loadImage (sprintf "racetrack/%i-%i.png" y x))
      ChariotTexture = loadImage "chariot"
      Font = loadImage "font"
      PlacingBackground = loadImage "placingbg"
      Particle = loadImage "particle"
      Button = loadImage "button"
      
      ChariotSound =
        let snd = loadSound "chariot"
        List.init playerQuantity (fun _ -> snd.CreateInstance())
      CrowdCheerSound = loadSoundInstance "cheer1"
    }