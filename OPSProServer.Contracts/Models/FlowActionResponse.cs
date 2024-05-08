using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class FlowActionResponse
    {
        public Guid FlowId { get; set; }
        public bool Accepted { get; }
        public List<Guid> CardsId { get; }

        [JsonConstructor]
        public FlowActionResponse(Guid flowId, bool accepted, List<Guid> cardsId)
        {
            FlowId = flowId;
            Accepted = accepted;
            CardsId = cardsId;
        }

        public FlowActionResponse(Guid flowId, bool accepted) : this(flowId, accepted, new List<Guid>()) { }
    }
}
