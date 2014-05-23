namespace CircusMaximus
open MonoMac.AppKit
open MonoMac.Foundation
open SDL2

type AppDelegate() = 
  inherit NSApplicationDelegate()
  
  override this.FinishedLaunching notification =
    using (new GameWindow()) (fun game -> game.Run ())
  
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
