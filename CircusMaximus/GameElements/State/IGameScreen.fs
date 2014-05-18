namespace CircusMaximus.Types
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

type GameInput = (MouseState * MouseState) * (KeyboardState * KeyboardState) * (GamePadState list * GamePadState list)

type GameFields =
  { settings: GameSettings
    rand: Random
    playerData: PlayerData list
    sounds: GameSounds }

type IGameScreen =
  abstract member Next: GameFields -> GameInput -> (IGameScreen * GameFields) option