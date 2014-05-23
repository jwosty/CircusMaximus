namespace CircusMaximus
open MonoMac.AppKit
open MonoMac.Foundation
open SDL2

type AppDelegate() = 
  inherit NSApplicationDelegate()
  
  override this.FinishedLaunching notification =
    let game = new GameWindow()
    game.Run ()
    // For some reason, MG-SDL2 doesn't seem to close the SDL2 window when the games exits, which causes the application to stay open
    SDL2.SDL.SDL_DestroyWindow <| SDL2.SDL.SDL_GL_GetCurrentWindow ()
  
  override this.ApplicationShouldTerminateAfterLastWindowClosed(sender) =
    true
 
module main =
  [<EntryPoint>]
  let main args =
    NSApplication.Init ()
    using (new NSAutoreleasePool()) (fun n -> 
      NSApplication.SharedApplication.Delegate <- new AppDelegate()
      NSApplication.Main args )
    0
