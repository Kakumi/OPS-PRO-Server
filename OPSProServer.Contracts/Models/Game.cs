﻿using OPSProServer.Contracts.Exceptions;
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

        //Mainly for client since this method is redefined inside GameRuleEngine with more details
        public bool CanAttack(Guid userId, Guid attacker)
        {
            var myGameInfo = GetMyPlayerInformation(userId);
            var attackerCard = myGameInfo.GetAttacker(attacker);

            return Turn > 1 && attackerCard != null && (attackerCard.Turn > 1 || attackerCard.CardInfo.IsRush) && !attackerCard.Rested;
        }
    }
}
