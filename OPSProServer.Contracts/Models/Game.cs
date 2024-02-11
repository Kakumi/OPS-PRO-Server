using OPSProServer.Contracts.Exceptions;
using OPSProServer.Contracts.Models.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class Game
    {
        public Guid Id { get; private set; }
        public GameState State { get; private set; }
        public int Turn { get; private set; }
        public Guid PlayerTurn { get; private set; }
        public Guid FirstToPlay { get; private set; }
        public PlayerGameInformation CreatorGameInformation { get; private set; }
        public PlayerGameInformation OpponentGameInformation { get; private set; }

        public event EventHandler<PhaseChangedArgs>? PhaseChanged;
        public event EventHandler<PlayerChangedArgs>? PlayerChanged;

        [JsonConstructor]
        public Game(Guid id, GameState state, int turn, Guid playerTurn, Guid firstToPlay, PlayerGameInformation creatorGameInformation, PlayerGameInformation opponentGameInformation)
        {
            Id = id;
            State = state;
            Turn = turn;
            PlayerTurn = playerTurn;
            FirstToPlay = firstToPlay;
            CreatorGameInformation = creatorGameInformation;
            OpponentGameInformation = opponentGameInformation;
        }

        public Game(Guid firstToPlay, PlayerGameInformation creatorGameInformation, PlayerGameInformation opponentGameInformation)
        {
            Id = Guid.NewGuid();
            State = GameState.Starting;
            Turn = 0;
            PlayerTurn = firstToPlay;
            FirstToPlay = firstToPlay;
            CreatorGameInformation = creatorGameInformation;
            OpponentGameInformation = opponentGameInformation;
        }

        public RuleResponse? CheckUpdatePhaseState(bool goNext = false)
        {
            var response = new RuleResponse();
            var currentPlayerInfo = GetCurrentPlayerGameInformation();
            var canGoNext = goNext || currentPlayerInfo.CurrentPhase!.IsAutoNextPhase();
            if (currentPlayerInfo.CurrentPhase!.State == PhaseState.Ending)
            {
                var oldPhase = currentPlayerInfo.CurrentPhase;
                var newPhase = currentPlayerInfo.CurrentPhase!.NextPhase();
                newPhase.State = PhaseState.Active;
                currentPlayerInfo.CurrentPhase = newPhase;
                response.Add(currentPlayerInfo.GetBoard().Select(x => x.Script.OnPhaseBegin(currentPlayerInfo.User, currentPlayerInfo, this, newPhase.PhaseType)));
                response.Add(currentPlayerInfo.CurrentPhase.OnPhaseStarted(currentPlayerInfo, this));

                if (newPhase.PhaseType == PhaseType.Opponent && oldPhase.PhaseType == PhaseType.End)
                {
                    var nextPlayerResponse = NextPlayer();
                    if (nextPlayerResponse != null)
                    {
                        response.Add(nextPlayerResponse);
                    }
                }
            } else if (currentPlayerInfo.CurrentPhase.State == PhaseState.Active)
            {
                if (canGoNext)
                {
                    currentPlayerInfo.CurrentPhase.State = PhaseState.Beginning;
                } else
                {
                    return null;
                }
            } else
            {
                response.Add(currentPlayerInfo.CurrentPhase.OnPhaseEnded(currentPlayerInfo, this));
                response.Add(currentPlayerInfo.GetBoard().Select(x => x.Script.OnPhaseEnd(currentPlayerInfo.User, currentPlayerInfo, this, currentPlayerInfo.CurrentPhase.PhaseType)));
                currentPlayerInfo.CurrentPhase.State = PhaseState.Ending;
            }

            return response;
        }

        public RuleResponse? NextPlayer()
        {
            Guid oldPlayerId = PlayerTurn;
            Guid newPlayerId = GetOpponentPlayerInformation(PlayerTurn).User.Id;
            PlayerTurn = newPlayerId;
            GetCurrentPlayerGameInformation().CurrentPhase!.State = PhaseState.Ending;

            PlayerChanged?.Invoke(this, new PlayerChangedArgs(oldPlayerId, newPlayerId, this));

            return CheckUpdatePhaseState();
        }

        public PlayerGameInformation GetCurrentPlayerGameInformation()
        {
            if (PlayerTurn == CreatorGameInformation.User.Id)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetMyPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.User.Id)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetOpponentPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.User.Id)
            {
                return OpponentGameInformation;
            }

            return CreatorGameInformation;
        }

        public void IncrementTurn()
        {
            Turn++;
        }

        public Result CanAttack(PlayingCard? card, User user)
        {
            var errors = new List<OPSException>();

            if (card == null)
            {
                errors.Add(new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND"));
            }

            if (Turn <= 0) //TODO 1
            {
                errors.Add(new ErrorUserActionException(user.Id, "GAME_PLAYER_CANT_ATTACK_FIRST_TURN"));
            }

            if (card.Turn <= 0) //TODO 1
            {
                if (!card.IsRush(user, this))
                {
                    errors.Add(new ErrorUserActionException(user.Id, "GAME_PLAYER_CHARACTER_CANT_ATTACK_FIRST_TURN"));
                }
            }

            if (card.Rested)
            {
                errors.Add(new ErrorUserActionException(user.Id, "GAME_PLAYER_CHARACTER_CANT_ATTACK_RESTED"));
            }

            return new Result(errors);
        }

        public List<PlayingCard> GetAttackableCards(User user)
        {
            var opponentGameInfo = GetOpponentPlayerInformation(user.Id);
            var cards = new List<PlayingCard>();
            cards.Add(opponentGameInfo.Leader);
            cards.AddRange(opponentGameInfo.GetCharacters().Where(x => x.Rested));
            if (cards.Count == 0)
            {
                throw new ErrorUserActionException(user.Id, "GAME_NO_CARDS_TO_ATTACK");
            }

            return cards;
        }

        public List<PlayingCard> GetCounterCards(User user)
        {
            var gameInfo = GetMyPlayerInformation(user.Id);
            return gameInfo.Hand.Where(x => x.GetTotalCounter() != 0).ToList();
        }

        public List<PlayingCard> GetBlockerCards(User user)
        {
            var gameInfo = GetMyPlayerInformation(user.Id);
            return gameInfo.GetCharacters().Where(x => x.IsBlocker(user, this)).ToList();
        }

        public List<PlayingCard> GetEventCounterCards(User toUser)
        {
            var gameInfo = GetMyPlayerInformation(toUser.Id);
            return gameInfo.GetCharacters().Where(x => x.CardInfo.IsEventCounter).ToList();
        }

        public Result CanSummon(User user, Guid cardId)
        {
            var errors = new List<OPSException>();
            var gameInfo = GetMyPlayerInformation(user.Id);
            var handCard = gameInfo.Hand.FirstOrDefault(x => x.Id == cardId);
            if (handCard != null)
            {
                if (gameInfo.DonAvailable < handCard.GetTotalCost())
                {
                    errors.Add(new ErrorUserActionException(user.Id, "GAME_NOT_ENOUGH_DON_CARDS", gameInfo.DonAvailable.ToString(), handCard.GetTotalCost().ToString()));
                }

                if (handCard.CardInfo.CardCategory == CardCategory.LEADER || handCard.CardInfo.CardCategory == CardCategory.EVENT)
                {
                    errors.Add(new ErrorUserActionException(user.Id, "GAME_CARD_CANNOT_BE_SUMMONED"));
                }

                if (handCard.CardInfo.CardCategory == CardCategory.CHARACTER && !gameInfo.HasEmptyCharacter())
                {
                    errors.Add(new ErrorUserActionException(user.Id, "GAME_CHARACTERS_FULL"));
                }
            } else
            {
                errors.Add(new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND"));
            }

            return new Result(errors);
        }

        public RuleResponse Summon(User user, Guid cardId, Guid replaceId = default)
        {
            var response = new RuleResponse();

            //Can summon (enough spaces) or replace character id is set
            var resultSummon = CanSummon(user, cardId);
            if (!resultSummon.Success && replaceId == default)
            {
                resultSummon.ThrowIfError();
                throw new ErrorUserActionException(user.Id, "GAME_CARD_CANNOT_BE_SUMMONED");
            }

            var gameInfo = GetMyPlayerInformation(user.Id);
            var handCard = gameInfo.Hand.First(x => x.Id == cardId);

            if (handCard.CardInfo.CardCategory == CardCategory.STAGE)
            {
                var trashCard = gameInfo.SetStage(handCard);
                response.Add(OnPlay(user, handCard));

                if (trashCard != null)
                {
                    response.Add(OnTrash(user, trashCard));
                }
            }
            else if (handCard.CardInfo.CardCategory == CardCategory.CHARACTER)
            {
                if (gameInfo.HasEmptyCharacter())
                {
                    _ = gameInfo.SetFirstEmptyCharacters(handCard);
                    response.Add(OnPlay(user, handCard));
                }
                else
                {
                    var replacedCard = gameInfo.ReplaceCharacter(handCard, replaceId);
                    if (replacedCard == null)
                    {
                        throw new ErrorUserActionException(user.Id, "GAME_CHARACTERS_FULL");
                    }

                    gameInfo.ReplaceCharacter(handCard, replaceId);
                    response.Add(OnPlay(user, handCard));
                    response.Add(OnTrash(user, replacedCard));

                    response.FlowResponses.Add(new FlowResponseMessage("GAME_CARD_TRASH", user.Username, replacedCard.CardInfo.Name));
                }
            }

            gameInfo.UseDonCard(handCard.GetTotalCost());
            gameInfo.RemoveFromHand(cardId);
            response.FlowResponses.Add(new FlowResponseMessage("GAME_PLAYER_SUMMONED", user.Username, handCard.CardInfo.Name, handCard.GetTotalCost().ToString()));

            return response;
        }

        public RuleResponse UseCounters(User user, Guid fromCardId, List<Guid> cardsId)
        {
            var response = new RuleResponse();
            var gameInfo = GetMyPlayerInformation(user.Id);

            var card = gameInfo.GetCharacterOrLeader(fromCardId);
            if (card == null)
            {
                throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
            }

            var counters = GetCounterCards(user);
            var cardsToUse = counters.Where(x => cardsId.Contains(x.Id)).ToList();
            foreach (var cardToUse in cardsToUse)
            {
                var removed = gameInfo.RemoveFromHand(cardToUse.Id);
                if (removed != null)
                {
                    response.Add(OnCounter(user, removed));
                    gameInfo.TrashCard(removed);
                    response.Add(OnTrash(user, removed));

                    response.FlowResponses.Add(new FlowResponseMessage("GAME_USE_COUNTER", user.Username, cardToUse.CardInfo.Name, cardToUse.CardInfo.Counter.ToString()));
                    card.PowerModifier.Add(new ValueModifier(ModifierDuration.Battle, cardToUse.GetTotalCounter()));
                }
            }

            return response;
        }

        public RuleResponse UseEventCounters(User user, List<Guid> cardsId)
        {
            var response = new RuleResponse();
            var gameInfo = GetMyPlayerInformation(user.Id);
            var counters = GetEventCounterCards(user);
            var cardsToUse = counters.Where(x => cardsId.Contains(x.Id)).ToList();

            foreach (var cardToUse in cardsToUse)
            {
                if (cardToUse.GetTotalCost() <= gameInfo.DonAvailable)
                {
                    var removed = gameInfo.RemoveFromHand(cardToUse.Id);
                    if (removed != null)
                    {
                        gameInfo.UseDonCard(cardToUse.GetTotalCost());
                        response.Add(OnCost(user, cardToUse));

                        response.Add(OnEventCounter(user, removed));
                        gameInfo.TrashCard(removed);
                        response.Add(OnTrash(user, removed));

                        response.FlowResponses.Add(new FlowResponseMessage("GAME_USE_EVENT_COUNTER", user.Username, cardToUse.CardInfo.Name, cardToUse.CardInfo.Cost.ToString()));
                    }
                } else
                {
                    response.FlowResponses.Add(new FlowResponseMessage(user, "GAME_USE_EVENT_COUNTER_NO_DON", user.Username, cardToUse.CardInfo.Name, cardToUse.CardInfo.Cost.ToString(), gameInfo.DonAvailable.ToString()));
                }
            }

            return response;
        }

        public RuleResponse<AttackResult> Attack(User user, User opponent, Guid attacker, Guid target)
        {
            var response = new RuleResponse<AttackResult>();

            var myGameInfo = GetMyPlayerInformation(user.Id);
            var opponentGameInfo = GetOpponentPlayerInformation(user.Id);
            var attackerCard = myGameInfo.GetCharacterOrLeader(attacker);
            var defenderCard = opponentGameInfo.GetCharacterOrLeader(target);
            if (attackerCard != null && defenderCard != null)
            {
                var attackTotalPower = attackerCard.GetTotalPower();
                var defenseTotalPower = defenderCard.GetTotalPower();

                response.Add(OnAttack(user, defenderCard));

                attackerCard.Rested = true;

                PlayingCard? lifeCard = null;
                bool winner = false;
                bool success = false;
                if (attackTotalPower >= defenseTotalPower)
                {
                    response.FlowResponses.Add(new FlowResponseMessage("GAME_PLAYER_ATTACK_SUCCESS", user.Username, opponent.Username, attackerCard.CardInfo.Name, defenderCard.CardInfo.Name, attackTotalPower.ToString(), defenseTotalPower.ToString()));
                    success = true;

                    if (defenderCard.Rested)
                    {
                        opponentGameInfo.KillCharacter(target);
                        response.Add(OnKO(opponent, defenderCard));
                    }

                    if (defenderCard.CardInfo.CardCategory == CardCategory.LEADER)
                    {
                        if (opponentGameInfo.Lifes.Count > 0)
                        {
                            lifeCard = opponentGameInfo.RemoveLifeCard();
                            if (lifeCard.CardInfo.IsTrigger)
                            {
                                var flowAction = new FlowAction(user, opponent, UseOrAddLifeCard);
                                var flowRequest = new FlowActionRequest(flowAction.Id, opponent, "GAME_ASK_LIFECARD", new List<Guid>() { lifeCard.Id }, 0, 1, true);
                                flowAction.Request = flowRequest;
                                flowAction.SetFromCardId(lifeCard.Id);
                            } else
                            {
                                response.FlowResponses.Add(new FlowResponseMessage("GAME_GET_LIFE_CARD", lifeCard.CardInfo.Name));
                                response.FlowResponses.Add(new FlowResponseMessage("GAME_GET_LIFE_CARD_ATTACKER", opponent.Username));
                                opponentGameInfo.AddHandCard(lifeCard);
                                response.Add(OnAddLifeCardToHand(opponent, lifeCard));
                            }

                            response.FlowResponses.Add(new FlowResponseMessage("GAME_PLAYER_LOOSE_LIFE", opponent.Username, opponentGameInfo.Lifes.Count().ToString()));
                        }
                        else
                        {
                            response.Winner = user;
                            winner = true;
                        }
                    }
                } else
                {
                    response.FlowResponses.Add(new FlowResponseMessage("GAME_PLAYER_ATTACK_FAILED", myGameInfo.User.Username, opponentGameInfo.User.Username, attackerCard.CardInfo.Name, defenderCard.CardInfo.Name, attackerCard.GetTotalPower().ToString(), defenderCard.GetTotalPower().ToString()));
                }

                attackerCard.RemoveStatDuration(ModifierDuration.Attack);
                defenderCard.RemoveStatDuration(ModifierDuration.Defense);
                attackerCard.RemoveStatDuration(ModifierDuration.Battle);
                defenderCard.RemoveStatDuration(ModifierDuration.Battle);

                response.Data = new AttackResult(myGameInfo, opponentGameInfo, attackerCard, defenderCard, lifeCard, attackTotalPower, defenseTotalPower, success, winner);

                return response;
            }

            throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
        }

        private RuleResponse UseOrAddLifeCard(FlowArgs args)
        {
            return new RuleResponse();
            return args.Room.Game!.UseCounters(args.User, args.FlowAction.ToCardId!.Value, args.Response.CardsId);
        }

        public RuleResponse GiveDon(User user, Guid cardId)
        {
            var response = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            if (gameInfo.DonAvailable == 0)
            {
                throw new ErrorUserActionException(user.Id, "GAME_NOT_ENOUGH_DON", "1");
            }

            var card = gameInfo.GetCharacterOrLeader(cardId);
            if (card == null)
            {
                throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
            }

            if (gameInfo.UseDonCard(1))
            {
                card.DonCard++;
            }

            response.Add(OnGiveDon(user, card));

            response.FlowResponses.Add(new FlowResponseMessage("GAME_PLAYER_CHARACTER_DON_USED", user.Username, "1", card.CardInfo.Name, card.GetTotalPower().ToString()));

            return response;
        }

        public RuleResponse OnCounter(User user, PlayingCard card)
        {
            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            var ruleResponse = card.Script.OnPlay(user, gameInfo, this, card, card);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnCounter(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnCounter(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnEventCounter(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();
            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnActivateCounterEvent(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnActivateCounterEvent(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnPlay(User user, PlayingCard card)
        {
            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            var ruleResponse = card.Script.OnPlay(user, gameInfo, this, card, card);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnSummoned(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnSummoned(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnTrash(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnTrash(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnTrash(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnGiveDon(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnGiveDon(user, gameInfo, this, x, card)));
            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnDonUsed(user, gameInfo, this, 1)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnGiveDon(user, opponentInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnDonUsed(user, opponentInfo, this, 1)));

            return ruleResponse;
        }

        public RuleResponse OnAttack(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnAttack(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnAttack(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnKO(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnKO(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnKO(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnAddLifeCardToHand(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnGetLifeCard(user, gameInfo, this, x, card)));
            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnAddLifeCardToHand(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnGetLifeCard(user, opponentInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnAddLifeCardToHand(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnUseBlocker(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnBlock(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnBlock(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }

        public RuleResponse OnCost(User user, PlayingCard card)
        {
            var ruleResponse = new RuleResponse();

            var gameInfo = GetMyPlayerInformation(user.Id);
            var opponentInfo = GetOpponentPlayerInformation(user.Id);

            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnDonUsed(user, gameInfo, this, card.GetTotalCost())));
            ruleResponse.Add(gameInfo.GetBoard().Select(x => x.Script.OnCost(user, gameInfo, this, x, card)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnDonUsed(user, opponentInfo, this, card.GetTotalCost())));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnCost(user, opponentInfo, this, x, card)));

            return ruleResponse;
        }
    }
}
