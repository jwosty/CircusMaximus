namespace CircusMaximus.Types
open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open CircusMaximus.HelperFunctions
open CircusMaximus.Types.UnitSymbols

type MouseInput(xnaMouseState: MouseState) =
  member this.xnaMouseState = xnaMouseState
  member this.Position = (xnaMouseState.X @@ xnaMouseState.Y) * 1.<px>

type GameInput =
  { lastKeyboard: KeyboardState
    keyboard: KeyboardState
    lastMouse: MouseInput
    mouse: MouseInput
    lastGamepads: GamePadState list
    gamepads: GamePadState list }
  
  /// Shifts this's current fields to a new input's old fields and uses the parameters for the new input's current fields
  member this.shift keyboard mouse gamepads =
    { this with
        lastKeyboard = this.keyboard; keyboard = keyboard
        lastMouse = this.mouse; mouse = mouse
        lastGamepads = this.gamepads; gamepads = gamepads }
  
  /// Starts off the game input for when the previous values are not available (e.g. when the game initializes)
  static member initInitial keyboard mouse gamepads =
    { lastKeyboard = keyboard; keyboard = keyboard
      lastMouse = mouse; mouse = mouse
      lastGamepads = gamepads; gamepads = gamepads }

type GameFields =
  { settings: GameSettings
    rand: Random
    playerData: PlayerData list
    sounds: GameSounds }

type IGameScreen =
  abstract member Next: deltaTime:float<s> -> GameFields -> GameInput -> (IGameScreen * GameFields) option