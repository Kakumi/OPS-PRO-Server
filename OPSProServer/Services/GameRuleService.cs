using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Contracts.Models;
using OPSProServer.Hubs;
using OPSProServer.Managers;
using OPSProServer.Models;

namespace OPSProServer.Services
{
    public class GameRuleService : IGameRuleService
    {
        private readonly ICardService _cardService;

        public GameRuleService(ICardService cardService)
        {
            _cardService = cardService;
        }

        public bool CanAttack(PlayingCard? card, User user, Room room, Game game)
        {
            if (card == null)
            {
                throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
            }

            if (game.Turn <= 0) //TODO 1
            {
                throw new ErrorUserActionException(user.Id, "GAME_PLAYER_CANT_ATTACK_FIRST_TURN");
            }

            if (card.Turn <= 0) //TODO 1
            {
                if (!card.CardInfo.IsRush)
                {
                    var script = _cardService.GetCardScript(card);
                    if (script == null)
                    {
                        throw new ErrorUserActionException(user.Id, "GAME_PLAYER_CHARACTER_CANT_ATTACK_FIRST_TURN");

                    }
                    else if (script != null && !script.IsRusher(user, game, card))
                    {
                        throw new ErrorUserActionException(user.Id, "GAME_PLAYER_CHARACTER_CANT_ATTACK_FIRST_TURN");

                    }
                }
            }

            if (card.Rested)
            {
                throw new ErrorUserActionException(user.Id, "GAME_PLAYER_CHARACTER_CANT_ATTACK_RESTED");
            }

            return true;
        }

        public AttackResult Attack(User user, Room room, Game game, Guid attacker, Guid target)
        {
            var myGameInfo = game.GetMyPlayerInformation(user.Id);
            var opponentGameInfo = game.GetOpponentPlayerInformation(user.Id);
            var attackerCard = myGameInfo.GetCharacterOrLeader(attacker);
            var defenderCard = opponentGameInfo.GetCharacterOrLeader(target);
            if (attackerCard != null && defenderCard != null)
            {
                var attackTotalPower = attackerCard.GetTotalPower();
                var defenseTotalPower = defenderCard.GetTotalPower();
                attackerCard.Rested = true;

                PlayingCard? lifeCard = null;
                bool winner = false;
                bool success = false;
                if (attackTotalPower >= defenseTotalPower)
                {
                    success = true;

                    if (defenderCard.Rested)
                    {
                        opponentGameInfo.KillCharacter(target);
                    }

                    if (defenderCard.CardInfo.CardCategory == CardCategory.LEADER)
                    {
                        if (opponentGameInfo.Lifes.Count > 0)
                        {
                            lifeCard = opponentGameInfo.RemoveLifeCard();
                        }
                        else
                        {
                            winner = true;
                        }
                    }
                }

                attackerCard.RemoveStatDuration(ModifierDuration.Attack);
                defenderCard.RemoveStatDuration(ModifierDuration.Defense);
                attackerCard.RemoveStatDuration(ModifierDuration.Battle);
                defenderCard.RemoveStatDuration(ModifierDuration.Battle);

                return new AttackResult(myGameInfo, opponentGameInfo, attackerCard, defenderCard, lifeCard, attackTotalPower, defenseTotalPower, success, winner);
            }

            throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
        }

        public Contracts.Models.RuleResponse GiveDon(User user, Room room, Game game, Guid cardId)
        {
            var response = new Contracts.Models.RuleResponse();

            var gameInfo = game.GetMyPlayerInformation(user.Id);
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

            var cardScript = _cardService.GetCardScript(card);
            if (cardScript != null)
            {
                cardScript.OnGiveDon(user, game, card);
            }

            response.FlowResponses.Add(new FlowResponseMessage(room, "GAME_PLAYER_CHARACTER_DON_USED", user.Username, "1", card.CardInfo.Name, card.GetTotalPower().ToString()));

            return new Contracts.Models.RuleResponse();
        }

        public List<PlayingCard> GetAttackableCards(User user, Room room, Game game)
        {
            var opponentGameInfo = game.GetOpponentPlayerInformation(user.Id);
            var cards = new List<PlayingCard>();
            cards.Add(opponentGameInfo.Leader);
            cards.AddRange(opponentGameInfo.GetCharacters().Where(x => x.Rested));
            if (cards.Count == 0)
            {
                throw new ErrorUserActionException(user.Id, "GAME_NO_CARDS_TO_ATTACK");
            }

            return cards;
        }

        public List<PlayingCard> GetCounterCards(User user, Room room, Game game)
        {
            var gameInfo = game.GetMyPlayerInformation(user.Id);
            return gameInfo.Hand.Where(x => x.GetTotalCounter() != 0).ToList();
        }

        public List<PlayingCard> GetBlockerCards(User user, Room room, Game game)
        {
            var gameInfo = game.GetMyPlayerInformation(user.Id);
            return gameInfo.GetCharacters().Where(x => _cardService.IsBlocker(x, user, game)).ToList();
        }

        public bool CanSummon(User user, Room room, Game game, Guid cardId)
        {
            var gameInfo = game.GetMyPlayerInformation(user.Id);
            var handCard = gameInfo.Hand.FirstOrDefault(x => x.Id == cardId);
            if (handCard != null)
            {
                if (gameInfo.DonAvailable < handCard.GetTotalCost())
                {
                    throw new ErrorUserActionException(user.Id, "GAME_NOT_ENOUGH_DON_CARDS", gameInfo.DonAvailable.ToString(), handCard.GetTotalCost().ToString());
                }

                if (handCard.CardInfo.CardCategory == CardCategory.LEADER || handCard.CardInfo.CardCategory == CardCategory.EVENT)
                {
                    throw new ErrorUserActionException(user.Id, "GAME_CARD_CANNOT_BE_SUMMONED");
                }

                if (handCard.CardInfo.CardCategory == CardCategory.CHARACTER && !gameInfo.HasEmptyCharacter())
                {
                    return false;
                }

                return true;
            }

            throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
        }

        public Contracts.Models.RuleResponse Summon(User user, Room room, Game game, Guid cardId, Guid replaceId = default)
        {
            var response = new Contracts.Models.RuleResponse();

            //Can summon (enough spaces) or replace character id is set
            if (CanSummon(user, room, game, cardId) || replaceId != default)
            {
                var gameInfo = game.GetMyPlayerInformation(user.Id);
                var handCard = gameInfo.Hand.First(x => x.Id == cardId);

                if (handCard.CardInfo.CardCategory == CardCategory.STAGE)
                {
                    var trashCard = gameInfo.SetStage(handCard);
                    if (trashCard != null)
                    {
                        response.FlowResponses.Add(new FlowResponseMessage(room, "GAME_CARD_TRASH", user.Username, trashCard.CardInfo.Name));
                        var script = _cardService.GetCardScript(trashCard);
                        if (script != null)
                        {
                            script.OnTrash(user, game, trashCard);
                        }
                    }
                }
                else if (handCard.CardInfo.CardCategory == CardCategory.CHARACTER)
                {
                    if (gameInfo.HasEmptyCharacter())
                    {
                        _ = gameInfo.SetFirstEmptyCharacters(handCard);
                    } else
                    {
                        var replacedCard = gameInfo.ReplaceCharacter(handCard, replaceId);
                        if (replacedCard == null)
                        {
                            throw new ErrorUserActionException(user.Id, "GAME_CHARACTERS_FULL");
                        }

                        gameInfo.ReplaceCharacter(handCard, replaceId);
                        response.FlowResponses.Add(new FlowResponseMessage(room, "GAME_CARD_TRASH", user.Username, replacedCard.CardInfo.Name));
                        var script = _cardService.GetCardScript(replacedCard);
                        if (script != null)
                        {
                            script.OnTrash(user, game, replacedCard);
                        }
                    }
                }

                gameInfo.UseDonCard(handCard.GetTotalCost());
                gameInfo.RemoveFromHand(cardId);
                response.FlowResponses.Add(new FlowResponseMessage(room, "GAME_PLAYER_SUMMONED", user.Username, handCard.CardInfo.Name, handCard.GetTotalCost().ToString()));

                return response;
            }

            throw new ErrorUserActionException(user.Id, "GAME_CARD_CANNOT_BE_SUMMONED");
        }

        public Contracts.Models.RuleResponse UseCounters(User user, Room room, Game game, Guid fromCardId, List<Guid> cardsId)
        {
            var response = new Contracts.Models.RuleResponse();
            var gameInfo = game.GetMyPlayerInformation(user.Id);

            var card = gameInfo.GetCharacterOrLeader(fromCardId);
            if (card == null)
            {
                throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
            }

            var counters = GetCounterCards(user, room, game);
            var cardsToUse = counters.Where(x => cardsId.Contains(x.Id)).ToList();
            foreach(var cardToUse in cardsToUse)
            {
                var removed = gameInfo.RemoveFromHand(cardToUse.Id);
                if (removed != null)
                {
                    response.FlowResponses.Add(new FlowResponseMessage(room, "GAME_USE_COUNTER", user.Username, cardToUse.CardInfo.Name, cardToUse.CardInfo.Counter.ToString()));
                    card.PowerModifier.Add(new ValueModifier(ModifierDuration.Battle, cardToUse.GetTotalCounter()));
                }
            }

            return response;
        }
    }
}
