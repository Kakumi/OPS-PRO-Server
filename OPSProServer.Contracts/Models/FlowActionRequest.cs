using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class FlowActionRequest
    {
        public Guid FlowActionId { get; }
        public User UserTarget { get; }
        public string CodeMessage { get; }
        public List<Guid> CardsId { get; }
        public FlowActionType Type { get; }
        public int MinSelection { get; }
        public int MaxSelection { get; }
        public bool Cancellable { get; }

        [JsonConstructor]
        public FlowActionRequest(Guid flowActionId, User userTarget, string codeMessage, List<Guid> cardsId, FlowActionType type, int minSelection, int maxSelection, bool cancellable)
        {
            FlowActionId = flowActionId;
            UserTarget = userTarget;
            CodeMessage = codeMessage;
            CardsId = cardsId;
            Type = type;
            MinSelection = minSelection;
            MaxSelection = maxSelection;
            Cancellable = cancellable;
        }

        public FlowActionRequest(Guid flowActionId, User userTarget, string codeMessage, List<Guid> cardsId, int minSelection, int maxSelection, bool cancellable)
        {
            FlowActionId = flowActionId;
            UserTarget = userTarget;
            CodeMessage = codeMessage;
            CardsId = cardsId;
            MinSelection = minSelection;
            MaxSelection = maxSelection;
            Cancellable = cancellable;
            Type = FlowActionType.Selection;
        }

        public FlowActionRequest(Guid flowActionId, User userTarget, string codeMessage, bool cancellable)
        {
            FlowActionId = flowActionId;
            UserTarget = userTarget;
            CodeMessage = codeMessage;
            CardsId = new List<Guid>();
            MinSelection = 0;
            MaxSelection = 0;
            Cancellable = cancellable;
            Type = FlowActionType.Question;
        }
    }
}
