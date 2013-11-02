module CircusMaximus.Taunt
open System

// A short list of simple Latin taunts and their English translation
let taunts =
  [ "Festina, festina!",                                  "Hurry, hurry!";
    "[NO TRANSLATION]",                                   "You smell like your horse!";
    "Sum es tardior quam carri in Via Appia!",            "You're slower than the carts on the Appian Way!";
    "[NO TRANSLATION]",                                   "You're a cheater like Caeser!";
    "[NO TRANSLATION]",                                   "Is that your horse or is it Cerberus pulling your chariot?";
    "[NO TRANSLATION]",                                   "I can hop faster than your excuse for a horse!" ]

let random = new Random()

let pickTaunt () = fst taunts.[random.Next(taunts.Length)]