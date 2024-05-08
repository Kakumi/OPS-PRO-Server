This application is a server for the One Piece TCG game. It can be used with [OPS Pro](https://github.com/Kakumi/OPS-PRO).

This project is still under development and will enable us to develop a turn-based card game simulation application.
I started it to learn and try to understand how a turn-based card game works.

Support or contact me :
<a href="https://discord.gg/2Cr6UdskdQ"><img src="https://discordapp.com/api/guilds/1237756823474536458/widget.png?style=banner2" alt="Discord server"></a>

---

Jump to:
* [Dependencies](#dependencies)
* [Installation](#install)
* [Features](#features)
* [Todo & Ideas](#todo)
* [Known bugs](#known-bugs)
* [Developement](#dev)
* [License](#license)

# <a name=“dependencies”></a>Dependencies
* Visual Studio 2022
* .NET 6 & .NET 7
* SignalR
* [OPS Pro](https://github.com/Kakumi/OPS-PRO) (Optional - Client that integrate the server)

# <a name=“install”></a>Install
No installation file is available at the moment, as the application is still under development.

In the case of development, here's how to proceed.
* Download OPS Pro Server and open the solution.
* In `appsettings.Development.json` or `appsettings.Production.json` modify the line: `“CardsPath”: “path\to\cards.json”` to target the JSON card data file. **This file is generated via an external tool that will be made available later. In the meantime, you'll find the file [here](temp_files/cards.json).
* Launch OPS Pro Server (Profile: `Run Dev` or `Run Prod`)
	* Dev is used to automate certain actions without depending on other players.

# <a name=“features”></a>Features
* API to fetch cards.json
* SignalR (Websocket)
	* Login as a user
	* Manage rooms (List, Create, Kick, Leave, Join, Set password, Set description)
	* Manage game (Choose first player with Rock Paper Scissor, Redraw first hand, Initialize board, Official One Piece TCG rules, Abandon, List cards, Cards states, Cards infos, Cards bonus and malus, ...)
* Card script (There's a very easy way of integrating new scripts (rules and effects) for game cards with a multitude of events that the script can integrate. See `OPSProServer.Contracts/Models/Scripts/OP01/OP01-001.cs` for an example.)

# <a name=“todo”></a>TODO
* Fix some bugs
* Add database to allow more instances
* Add realtime chat
* Add missing cards scripts
* Allow spectators
* Fix and add unit tests
* Add timer to restrict player's turn
* Add rules to ban or restrict some cards

# <a name=“known-bugs”></a>Known Bugs
* Sometimes users are not logged out

# <a name=“dev”></a>Developement
The advantage of developing on this server with a public source code is that several applications can be grafted onto it, enabling players to play on any application that integrates the server.</br>
</br>
At the moment, there's no documentation on the development, but if it's necessary I could write it progressively if I see that it's of interest to several people.

# <a name=“license”></a>Licence
OPS Pro Server can be used by anyone for any purpose allowed by the permissive MIT License.