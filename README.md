# 5D Chess With Multiverse Time Travel Data Interface
![](https://github.com/GHXX/FiveDChessDataInterface/actions/workflows/dotnet.yml/badge.svg)

An **unofficial** C# (.Net Standard 2.0) library providing runtime memory-access to [5D Chess With Multiverse Time Travel](https://store.steampowered.com/app/1349230/5D_Chess_With_Multiverse_Time_Travel/).

## Disclaimer
While it should be stable for the most part, this library may still cause crashes/desyncs or other unexpected/unwanted behaviour, hence why the developers of this project cannot be held liable for any damage caused.


## Important notes
Currently only reading data is supported. 

While the library could be adapted to writing to the game's memory, it greatly increases the risk of crashing or desyncing; especially in online games, thus at this point no data is written to the game's memory. 
Future versions of the library may include such features. 

As a side note, cheating will never be possible, because of how the game is structured: 
All moves are checked clientside and invalid moves are simply discarded, meaning if you moved a rook diagonally, it would not move at all on the other player's screen.
Ontop of that, the goal of this project is not to create cheats for the game, instead it is aimed to support creating useful tools, such as a [chess clock that automatically switches when a player submits their moves](https://github.com/GHXX/FiveDChessClock).

## Example of using the library

The main object that is being used is the `DataInterface` object. Creating works as follows:

* You can create it using the 'new'-keyword, passing a `System.Diagnostics.Process` object, which will link the DataInterface object to that process.
* It's also possible to have the library automatically resolve the Game-process, but only if only if there is exactly one 5D Chess With Multiverse Time Travel process is running:
  * Call `DataInterface.CreateAutomatically()`, which will either return an instance if everything worked, or throw an exception.
  * Call `DataInterface.TryCreateAutomatically()`, which returns true on success, and passes back the created DataInterface object via an `out` parameter. Optionally it can also return the number of running game-processes, which will be 1 if true is returned.
  
Once you have acquired a `DataInterface` object, you can call its non-static methods, currently allowing you to read:
* The number of chessboards that exist currently: `DataInterface.GetChessBoardAmount()`
* (Not implemented yet) A list of all chessboard of the current game: `DataInterface.GetChessBoards()`
* The size of all chessboard: `DataInterface.GetChessBoardSize()`
* The current player's turn: `DataInterface.GetCurrentPlayersTurn()`
