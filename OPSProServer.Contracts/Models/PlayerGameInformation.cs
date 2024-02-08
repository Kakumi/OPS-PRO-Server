using OPSProServer.Contracts.Exceptions;
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
        public User User { get; private set; }
        public bool Waiting { get; set; }
        public DeckInfo SelectedDeck { get; private set; }
        public List<PlayingCard> Deck { get; private set; }
        public Stack<PlayingCard> Lifes { get; private set; }
        public List<PlayingCard> Trash { get; private set; }
        public List<PlayingCard> Hand { get; private set; }
        public int DonDeck { get; private set; }
        public int DonAvailable { get; private set; }
        public int DonRested { get; private set; }
        public PlayingCard?[] Characters { get; private set; }
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
        public PlayerGameInformation(Guid userId, string username, User user, bool waiting, DeckInfo selectedDeck, List<PlayingCard> deck, Stack<PlayingCard> lifes, List<PlayingCard> trash, List<PlayingCard> hand, int donDeck, int donAvailable, int donRested, PlayingCard[] characters, PlayingCard? stage, PlayingCard leader, PhaseType currentPhaseType, bool hasRedrawn)
        {
            UserId = userId;
            Username = username;
            User = user;
            Waiting = waiting;
            SelectedDeck = selectedDeck;
            Deck = deck;
            Lifes = lifes;
            Trash = trash;
            Hand = hand;
            DonDeck = donDeck;
            DonAvailable = donAvailable;
            DonRested = donRested;
            Characters = characters;
            Stage = stage;
            Leader = leader;
            CurrentPhaseType = currentPhaseType;
            HasRedrawn = hasRedrawn;
        }

        public bool HasRedrawn { get; set; }

        public PlayerGameInformation(User user, DeckInfo selectedDeck, IPhase phase)
        {
            HasRedrawn = false;

            UserId = user.Id;
            Username = user.Username;
            User = user;
            Waiting = false;
            SelectedDeck = selectedDeck;
            Deck = new List<PlayingCard>();
            Lifes = new Stack<PlayingCard>();
            Trash = new List<PlayingCard>();
            Hand = new List<PlayingCard>();
            DonDeck = 10;
            DonAvailable = 0;
            DonRested = 0;
            Characters = new PlayingCard[5];
            Stage = null;
            CurrentPhase = phase;

            var leaderCard = selectedDeck.GetLeader();

            Leader = new PlayingCard(leaderCard!);
            Leader.VisibleForOpponent = true;

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
            return Characters.Where(x => x != null).ToList()!;
        }

        public PlayingCard? GetCharacter(Guid id)
        {
            return Characters.FirstOrDefault(x => x != null && x.Id == id);
        }

        public PlayingCard? GetCharacterOrLeader(Guid id)
        {
            if (Leader != null && Leader.Id == id)
            {
                return Leader;
            }

            return GetCharacter(id);
        }

        public List<PlayingCard> GetCharactersOrLeader()
        {
            var tempList = new List<PlayingCard>();
            tempList.Add(Leader);
            tempList.AddRange(GetCharacters());
            return tempList;
        }

        public PlayingCard? KillCharacter(Guid id)
        {
            for(int i = 0; i < Characters.Length; i++)
            {
                var character = Characters[i];
                if (character != null && character.Id == id)
                {
                    TrashCard(character);
                    Characters[i] = null;
                    return character;
                }
            }

            return null;
        }

        public void AddDeckCard(PlayingCard playingCard)
        {
            playingCard.ResetTurn();
            playingCard.VisibleForOpponent = true;
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
            playingCard.ResetTurn();
            playingCard.VisibleForOpponent = false;
            Hand.Add(playingCard);
        }

        public PlayingCard? RemoveFromHand(Guid id)
        {
            var card = Hand.FirstOrDefault(x => x.Id == id);
            if (card != null)
            {
                card.VisibleForOpponent = true;
                Hand.RemoveAll(x => x.Id == id);
                return card;
            }

            return null;
        }

        public void AddLifeCard(PlayingCard playingCard)
        {
            playingCard.ResetTurn();
            playingCard.VisibleForOpponent = false;
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
            playingCard.ResetTurn();
            playingCard.VisibleForOpponent = true;
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
                case PhaseType.Opponent: return new OpponentPhase(false);
            }

            throw new NotImplementedException();
        }

        public bool HasEmptyCharacter()
        {
            return Characters.Any(x => x == null);
        }

        public bool SetFirstEmptyCharacters(PlayingCard playingCard)
        {
            if (HasEmptyCharacter())
            {
                for(int i = 0; i < Characters.Length; i++)
                {
                    if (Characters[i] == null)
                    {
                        playingCard.VisibleForOpponent = true;
                        Characters[i] = playingCard;
                        return true;
                    }
                }
            }

            return false;
        }

        public void IncrementCardsTurn()
        {
            Leader.IncrementTurn();
            if (Stage != null)
            {
                Stage.IncrementTurn();
            }

            foreach(var character in GetCharacters())
            {
                character.IncrementTurn();
            }
        }

        public PlayingCard? GetCard(Guid id)
        {
            PlayingCard? card = GetCharacterOrLeader(id);
            if (card == null)
            {
                if (Stage != null && Stage.Id == id)
                {
                    return Stage;
                }

                return Hand.FirstOrDefault(x => x.Id == id);
            }

            return card;
        }

        public PlayingCard? SetStage(PlayingCard card)
        {
            var trashCard = Stage;
            if (Stage != null)
            {
                TrashCard(Stage);
            }

            card.VisibleForOpponent = true;
            Stage = card;
            return trashCard;
        }

        public PlayingCard? ReplaceCharacter(PlayingCard card, Guid replaceId)
        {
            for (int i = 0; i < Characters.Length; i++)
            {
                var character = Characters[i];
                if (character != null && character.Id == replaceId)
                {
                    TrashCard(character);
                    card.VisibleForOpponent = true;
                    Characters[i] = card;
                    return character;
                }
            }

            return null;
        }

        internal List<PlayingCard> GetBoard()
        {
            var board = new List<PlayingCard>();
            board.AddRange(GetCharactersOrLeader());
            if (Stage != null)
            {
                board.Add(Stage);
            }

            return board;
        }
    }
}
