﻿using OPSProServer.Exceptions;

namespace OPSProServer.Models
{
    public class PlayerGameInformation
    {
        public Guid UserId { get; set; }
        public DeckInfo SelectedDeck { get; set; }
        public List<PlayingCard> Deck { get; set; }
        public Stack<PlayingCard> Lifes { get; set; }
        public List<PlayingCard> Trash { get; set; }
        public List<PlayingCard> Hand { get; set; }
        public int DonDeck { get; set; }
        public int DonAvailable { get; set; }
        public int DonRested { get; set; }
        public PlayingCard? Character1 { get; set; }
        public PlayingCard? Character2 { get; set; }
        public PlayingCard? Character3 { get; set; }
        public PlayingCard? Character4 { get; set; }
        public PlayingCard? Character5 { get; set; }
        public PlayingCard? Stage { get; set; }
        public PlayingCard Leader { get; set; }
        public IPhase CurrentPhase { get; set; }


        public bool HasRedrawn { get; set; }

        public List<PlayingCard> GetCharacters()
        {
            var list = new List<PlayingCard>();
            if (Character1 != null)
            {
                list.Add(Character1);
            }
            if (Character2 != null)
            {
                list.Add(Character2);
            }
            if (Character3 != null)
            {
                list.Add(Character3);
            }
            if (Character4 != null)
            {
                list.Add(Character4);
            }
            if (Character5 != null)
            {
                list.Add(Character5);
            }

            return list;
        }

        public PlayerGameInformation()
        {

        }

        public PlayerGameInformation(Guid userId, DeckInfo selectedDeck, IPhase phase)
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
            CurrentPhase = phase;

            var leaderCard = selectedDeck.GetLeader();

            Leader = new PlayingCard(leaderCard);

            Initialize(selectedDeck);
        }

        private void Initialize(DeckInfo deck)
        {
            var leaderCard = deck.GetLeader();
            var deckCards = deck.GetCards();
            foreach (var deckCard in deckCards)
            {
                AddDeckCard(new PlayingCard(deckCard));
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
            Deck = Deck.OrderBy(a => Guid.NewGuid()).ToList();
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
