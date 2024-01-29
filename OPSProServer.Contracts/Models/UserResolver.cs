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

        [JsonConstructor]
        public UserResolver(Guid id, Guid resolverId, ActionResolverType type, string codeMessage, List<Guid> cardsId)
        {
            Id = id;
            ResolverId = resolverId;
            Type = type;
            CodeMessage = codeMessage;
            CardsId = cardsId;
        }

        public UserResolver(Guid resolverId, ActionResolverType type, string codeMessage, List<Guid> cardsId)
        {
            Id = Guid.NewGuid();
            ResolverId = resolverId;
            Type = type;
            CodeMessage = codeMessage;
            CardsId = cardsId;
        }
    }
}
