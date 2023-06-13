using OPSProServer.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class PlayerGameInformation
    {
        public Guid UserId { get; }
        public DeckInfo SelectedDeck { get; }
        public List<PlayingCard> Deck { get; private set; }
        public Stack<PlayingCard> Lifes { get; private set; }
        public List<PlayingCard> Trash { get; private set; }
        public List<PlayingCard> Hand { get; private set; }
        public int DonDeck { get; private set; }
        public int DonAvailable { get; private set; }
        public int DonRested { get; private set; }
        public PlayingCard? Character1 { get; private set; }
        public PlayingCard? Character2 { get; private set; }
        public PlayingCard? Character3 { get; private set; }
        public PlayingCard? Character4 { get; private set; }
        public PlayingCard? Character5 { get; private set; }
        public PlayingCard? Stage { get; private set; }
        public PlayingCard Leader { get; private set; }
        public string CurrentPhase { get; private set; }
        public string NextPhase { get; private set; }

        public bool HasRedrawn { get; private set; }

        public PlayerGameInformation(Guid userId, DeckInfo selectedDeck)
        {
            HasRedrawn = false;

            UserId = userId;
            SelectedDeck = selectedDeck;
            Deck = new List<PlayingCard>();
            Lifes = new Stack<PlayingCard>();
            Trash = new List<PlayingCard>();
            Hand = new List<PlayingCard>();
            DonDeck = 10;
            DonAvailable = 0;
            DonRested = 0;
            Character1 = null;
            Character2 = null;
            Character3 = null;
            Character4 = null;
            Character5 = null;
            Stage = null;

            var leaderCard = selectedDeck.Cards.First(x => x.Key.CardCategory == CardCategory.LEADER).Key;

            Leader = new PlayingCard(leaderCard);

            Initialize(selectedDeck);
        }

        private void Initialize(DeckInfo deck)
        {
            var leaderCard = deck.Cards.First(x => x.Key.CardCategory == CardCategory.LEADER).Key;
            var deckCards = deck.Cards.Where(x => x.Key.CardCategory == CardCategory.CHARACTER || x.Key.CardCategory == CardCategory.STAGE || x.Key.CardCategory == CardCategory.EVENT);
            foreach (var deckCard in deckCards)
            {
                for (int i = 0; i < deckCard.Value; i++)
                {
                    AddDeckCard(new PlayingCard(deckCard.Key));
                }
            }

            ShuffleDeck();

            DrawCard(5);

            RemoveDeckCards(leaderCard.Cost).ForEach(x =>
            {
                AddLifeCard(x);
            });
        }

        public void Redraw()
        {
            if (!HasRedrawn)
            {
                HasRedrawn = true;
                Initialize(SelectedDeck);
            }
        }

        public void AddDeckCard(PlayingCard playingCard)
        {
            Deck.Add(playingCard);
        }

        private List<PlayingCard> RemoveDeckCards(int amount = 1)
        {
            if (Deck.Count >= amount)
            {
                var cards = Deck.Take(amount).ToList();
                Deck.RemoveRange(0, amount);

                return cards;
            }

            throw new GameFinishedException();
        }

        public void ShuffleDeck()
        {
            Deck = Deck.OrderBy(a => System.Guid.NewGuid()).ToList();
        }

        public List<PlayingCard> DrawCard(int amount = 1)
        {
            var cards = RemoveDeckCards(amount);
            cards.ForEach(x =>
            {
                if (x != null)
                {
                    AddHandCard(x);
                }
            });

            return cards;
        }

        public void AddHandCard(PlayingCard playingCard)
        {
            Hand.Add(playingCard);
        }

        public void AddLifeCard(PlayingCard playingCard)
        {
            Lifes.Push(playingCard);
        }

        public PlayingCard RemoveLifeCard()
        {
            return Lifes.Pop();
        }

        public void DrawDonCard(int amount = 1)
        {
            if (amount > DonDeck)
            {
                amount = DonDeck;
            }
            else if (amount < 0)
            {
                amount = 1;
            }

            if (amount != 0)
            {
                DonDeck -= amount;
                DonAvailable += amount;
            }
        }

        public bool UseDonCard(int amount = 1)
        {
            if (amount < 0)
            {
                amount = 1;
            }

            if (DonAvailable >= amount)
            {
                DonAvailable -= amount;
                DonRested += amount;

                return true;
            }

            return false;
        }

        public void UnrestCostDeck()
        {
            DonAvailable += DonRested;
            DonRested = 0;
        }

        public void TrashCard(PlayingCard playingCard)
        {
            Trash.Add(playingCard);
        }
    }
}
