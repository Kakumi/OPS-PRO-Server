using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class PlayerChangedArgs
    {
        public Guid OldPlayer { get; }
        public Guid NewPlayer { get; }
        public Game Game { get; }

        public PlayerChangedArgs(Guid oldPlayer, Guid newPlayer, Game game)
        {
            OldPlayer = oldPlayer;
            NewPlayer = newPlayer;
            Game = game;
        }
    }
}
