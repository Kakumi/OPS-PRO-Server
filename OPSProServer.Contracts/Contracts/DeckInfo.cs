using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class DeckInfo
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public Dictionary<CardInfo, int> Cards { get; }

        public DeckInfo(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Cards = new Dictionary<CardInfo, int>();
        }
    }
}
