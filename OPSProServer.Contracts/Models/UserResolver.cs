using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class UserResolver
    {
        public Guid Id { get; }
        public Guid ResolverId { get; }
        public ActionResolverType Type { get; }
        public string CodeMessage { get; }
        public List<Guid> CardsId { get; }
        public int MinSelection { get; }
        public int MaxSelection { get; }
        public bool Cancellable { get; }

        [JsonConstructor]
        public UserResolver(Guid id, Guid resolverId, ActionResolverType type, string codeMessage, List<Guid> cardsId, int minSelection, int maxSelection, bool cancellable)
        {
            Id = id;
            ResolverId = resolverId;
            Type = type;
            CodeMessage = codeMessage;
            CardsId = cardsId;
            MinSelection = minSelection;
            MaxSelection = maxSelection;
            Cancellable = cancellable;
        }

        public UserResolver(Guid resolverId, ActionResolverType type, string codeMessage, List<Guid> cardsId, int minSelection, int maxSelection, bool cancellable)
        {
            Id = Guid.NewGuid();
            ResolverId = resolverId;
            Type = type;
            CodeMessage = codeMessage;
            CardsId = cardsId;
            MinSelection = minSelection;
            MaxSelection = maxSelection;
            Cancellable = cancellable;
        }

        public UserResolver(Guid resolverId, ActionResolverType type, string codeMessage) : this(resolverId, type, codeMessage, new List<Guid>(), 0, 0, true) { }
    }
}
