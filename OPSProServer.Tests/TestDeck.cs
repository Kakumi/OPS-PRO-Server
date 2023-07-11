using OPSProServer.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Tests
{
    [TestClass]
    public class TestDeck
    {
        private CardInfo _leaderCard;
        private List<CardInfo> _redCards;
        private List<CardInfo> _blueCards;
        private CardInfo _sameCard;
        private DeckInfo _deckValid;
        private DeckInfo _deckWithOnlyLeaders;
        private DeckInfo _deckNoLeader;
        private DeckInfo _deckNotEnoughCards;
        private DeckInfo _deckNotSameColor;
        private DeckInfo _deckExceedsSameCard;

        [TestInitialize]
        public void Initialize()
        {
            _leaderCard = new CardInfo("leader", new List<string>(), "OP01-001", "L", "LEADER", "Zoro", 5, null, "STRIKE", 5000, 0, new List<string>() { "RED", "GREEN" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test");
            _redCards = new List<CardInfo>();
            _blueCards = new List<CardInfo>();
            _sameCard = new CardInfo(Guid.NewGuid().ToString(), new List<string>(), "OP01-002", "R", "CHARACTER", "Luffy", 5, null, "STRIKE", 5000, 0, new List<string>() { "RED" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test");

            for (int i = 0; i < 50; i++)
            {
                var redCard = new CardInfo(Guid.NewGuid().ToString(), new List<string>(), "OP01-002", "R", "CHARACTER", "Luffy", 5, null, "STRIKE", 5000, 0, new List<string>() { "RED" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test");
                _redCards.Add(redCard);

                var blueCard = new CardInfo(Guid.NewGuid().ToString(), new List<string>(), "OP01-002", "R", "CHARACTER", "Luffy", 5, null, "STRIKE", 5000, 0, new List<string>() { "BLUE" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test");
                _blueCards.Add(blueCard);
            }

            _deckValid = new DeckInfo("valid");
            _deckValid.AddCard(_leaderCard);
            _deckValid.Cards.AddRange(_redCards);

            _deckWithOnlyLeaders = new DeckInfo("valid");
            _deckWithOnlyLeaders.AddCard(_leaderCard, 50);

            _deckNoLeader = new DeckInfo("no leader");
            _deckNoLeader.Cards.AddRange(_redCards);

            _deckNotEnoughCards = new DeckInfo("not enough card");
            _deckNotEnoughCards.AddCard(_leaderCard);
            _deckNotEnoughCards.Cards.AddRange(_redCards);
            _deckNotEnoughCards.Cards.RemoveRange(40, 5);

            _deckNotSameColor = new DeckInfo("not same color");
            _deckNotSameColor.AddCard(_leaderCard);
            _deckNotEnoughCards.Cards.AddRange(_blueCards);

            _deckExceedsSameCard = new DeckInfo("exceeds same card");
            _deckExceedsSameCard.AddCard(_leaderCard);
            _deckExceedsSameCard.AddCard(_sameCard, 50);
        }

        [TestMethod]
        public void ValidateDecks()
        {
            Assert.IsTrue(_deckValid.IsValid());
            Assert.IsFalse(_deckWithOnlyLeaders.IsValid());
            Assert.IsFalse(_deckNoLeader.IsValid());
            Assert.IsFalse(_deckNotEnoughCards.IsValid());
            Assert.IsFalse(_deckNotSameColor.IsValid());
            Assert.IsFalse(_deckExceedsSameCard.IsValid());
        }
    }
}
