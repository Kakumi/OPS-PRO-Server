﻿using OPSProServer.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class PlayerGameInformation
    {
        public Guid UserId { get; private set; }
        public string Username { get; private set; }
        public DeckInfo SelectedDeck { get; private set; }
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
        private IPhase? _currentPhase;
        public IPhase? CurrentPhase
        {
            get => _currentPhase;
            internal set
            {
                _currentPhase = value;
                if (value != null)
                {
                    CurrentPhaseType = value.PhaseType;
                }
            }
        }
        private PhaseType _phaseType;
        public PhaseType CurrentPhaseType //Use for SignalR because we can't send Interface
        {
            get => _phaseType;
            set
            {
                _phaseType = value;
                if (CurrentPhase == null)
                {
                    CurrentPhase = GetPhase(value);
                }
            }
        }

        [JsonConstructor]
        public PlayerGameInformation(Guid userId, string username, DeckInfo selectedDeck, List<PlayingCard> deck, Stack<PlayingCard> lifes, List<PlayingCard> trash, List<PlayingCard> hand, int donDeck, int donAvailable, int donRested, PlayingCard? character1, PlayingCard? character2, PlayingCard? character3, PlayingCard? character4, PlayingCard? character5, PlayingCard? stage, PlayingCard leader, PhaseType currentPhaseType, bool hasRedrawn)
        {
            UserId = userId;
            Username = username;
            SelectedDeck = selectedDeck;
            Deck = deck;
            Lifes = lifes;
            Trash = trash;
            Hand = hand;
            DonDeck = donDeck;
            DonAvailable = donAvailable;
            DonRested = donRested;
            Character1 = character1;
            Character2 = character2;
            Character3 = character3;
            Character4 = character4;
            Character5 = character5;
            Stage = stage;
            Leader = leader;
            CurrentPhaseType = currentPhaseType;
            HasRedrawn = hasRedrawn;
        }

        public bool HasRedrawn { get; set; }

        public PlayerGameInformation(Guid userId, string username, DeckInfo selectedDeck, IPhase phase)
        {
            HasRedrawn = false;

            UserId = userId;
            Username = username;
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

            Leader = new PlayingCard(leaderCard!);

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

            RemoveDeckCards(leaderCard!.Cost).ForEach(x =>
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

        public IPhase GetPhase(PhaseType type)
        {
            switch (type)
            {
                case PhaseType.Refresh: return new RefreshPhase();
                case PhaseType.Don: return new DonPhase();
                case PhaseType.Draw: return new DrawPhase();
                case PhaseType.Main: return new MainPhase();
                case PhaseType.End: return new EndPhase();
                case PhaseType.Opponent: return new OpponentPhase();
            }

            throw new NotImplementedException();
        }
    }
}
