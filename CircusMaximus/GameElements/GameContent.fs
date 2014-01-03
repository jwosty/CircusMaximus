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
    Button: Texture2D
    Particle: Texture2D
    PlacingBackground: Texture2D
    AwardBackground: Texture2D
    
    ChariotSound: SoundEffectInstance list
    CrowdCheerSound: SoundEffectInstance }

[<AutoOpen>]
module GameContentFunctions =
  let loadImage (content: ContentManager) img = content.Load<Texture2D>("images/" + img)
  let loadSound (content: ContentManager) snd = content.Load<SoundEffect>("sounds/" + snd)
  let initSoundInstance (sound: SoundEffect) =
    let instance = sound.CreateInstance()
    instance.IsLooped <- false
    instance
  let loadSoundInstance content snd = snd |> (loadSound content) |> initSoundInstance
  
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
      Button = loadImage "button"
      Particle = loadImage "particle"
      PlacingBackground = loadImage "placingbg"
      AwardBackground = loadImage "awardbg"
      
      ChariotSound =
        let snd = loadSound "chariot"
        List.init playerQuantity (fun _ -> initSoundInstance snd)
      CrowdCheerSound = loadSoundInstance "cheer1"
    }