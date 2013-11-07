module CircusMaximus.Taunt
open System

// A short list of simple Latin taunts and their English translation
let taunts =
  [ "Festina, festina!",                                  "Hurry, hurry!";
    "Odorem equi habes!",                                 "You smell like your horse!";
    "Sum es tardior quam carri in Via Appia!",            "You're slower than the carts on the Appian Way!";
    "Equi vel Cereberus quadrigam trahunt?",              "Is that your horse or is it Cerberus pulling your chariot?";
    "Salio celerior quam equus tuus!",                    "I can hop faster than your horse!" ]

let random = new Random()

let pickTaunt () = fst taunts.[random.Next(taunts.Length)]