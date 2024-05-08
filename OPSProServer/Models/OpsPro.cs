using OPSProServer.Contracts.Models;
using System.Text.Json.Serialization;

namespace OPSProServer.Models
{
    public class OpsPro
    {
        public required string CardsPath { get; set; }
        //public required List<CardInfo> Cards { get; set; }
    }
}
