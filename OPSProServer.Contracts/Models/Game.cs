using OPSProServer.Contracts.Exceptions;
using System;
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

            CreatorGameInformation.CurrentPhase!.OnPhaseStarted(this);
            OpponentGameInformation.CurrentPhase!.OnPhaseStarted(this);
        }

        public async Task UpdatePhase()
        {
            var currentPlayerInfo = GetCurrentPlayerGameInformation();
            var oldPhase = currentPlayerInfo.CurrentPhase!;
            var newPhase = currentPlayerInfo.CurrentPhase!.NextPhase();

            currentPlayerInfo.CurrentPhase.OnPhaseEnded(this);
            currentPlayerInfo.CurrentPhase = newPhase;
            currentPlayerInfo.CurrentPhase.OnPhaseStarted(this);

            if (PhaseChanged != null)
            {
                var args = new PhaseChangedArgs(oldPhase.PhaseType, newPhase.PhaseType, this);
                PhaseChanged.Invoke(this, args);
                await args.WaitCompletion.Task;
            }

            if (currentPlayerInfo.CurrentPhase.IsAutoNextPhase())
            {
                await UpdatePhase();
            }
        }

        public async Task NextPlayer()
        {
            Guid oldPlayerId = PlayerTurn;
            Guid newPlayerId = GetOpponentPlayerInformation(PlayerTurn).UserId;
            PlayerTurn = newPlayerId;

            PlayerChanged?.Invoke(this, new PlayerChangedArgs(oldPlayerId, newPlayerId, this));

            await UpdatePhase();
        }

        public PlayerGameInformation GetCurrentPlayerGameInformation()
        {
            if (PlayerTurn == CreatorGameInformation.UserId)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetMyPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.UserId)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetOpponentPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.UserId)
            {
                return OpponentGameInformation;
            }

            return CreatorGameInformation;
        }

        public void IncrementTurn()
        {
            Turn++;
        }

        public bool CanAttack(Guid userId, Guid attacker)
        {
            var myGameInfo = GetMyPlayerInformation(userId);
            var attackerCard = myGameInfo.GetCharacter(attacker);

            //TODO Check si le joueur peut attaquer (nb tour > 1)
            //TODO Check si la carte peut attaqué le tour de son invocation (rush / nb tour carte > 1)
            return attackerCard != null && !attackerCard.Rested;
        }

        public AttackResult Attack(Guid userId, Guid attacker, Guid target)
        {
            var myGameInfo = GetMyPlayerInformation(userId);
            var opponentGameInfo = GetOpponentPlayerInformation(userId);
            var attackerCard = myGameInfo.GetCharacter(attacker);
            var defenderCard = opponentGameInfo.GetCharacter(target);
            if (attackerCard != null && defenderCard != null)
            {
                attackerCard.Rested = true;

                if (attackerCard.GetTotalPower() >= defenderCard.GetTotalPower())
                {
                    if (defenderCard.Rested)
                    {
                        opponentGameInfo.KillCharacter(target);
                    } else
                    {
                        defenderCard.Rested = true;
                    }

                    return new AttackResult(myGameInfo, opponentGameInfo, attackerCard, defenderCard, true);
                    //TODO Manage life points and potential trigger
                }

                return new AttackResult(myGameInfo, opponentGameInfo, attackerCard, defenderCard, false);
            }

            throw new ErrorUserActionException(userId, "GAME_CARD_NOT_FOUND");
        }
    }
}
