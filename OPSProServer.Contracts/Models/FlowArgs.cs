using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class FlowArgs
    {
        public User User { get; set; }
        public Room Room { get; set; }
        public Game Game { get; set; }
        public FlowAction FlowAction { get; set; }
        public FlowActionResponse Response { get; set; }

        public FlowArgs(User user, Room room, Game game, FlowAction flowAction, FlowActionResponse response)
        {
            User = user;
            Room = room;
            Game = game;
            FlowAction = flowAction;
            Response = response;
        }
    }
}
