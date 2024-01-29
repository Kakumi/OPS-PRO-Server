using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class ActionResolver
    {
        public Guid Id { get; }
        public DateTime Created { get; }
        public CardAction Action { get; }
        public Guid? FromCardId { get; set; }
        public Guid? ToCardId { get; set; }
        public Guid UserId { get; }

        [JsonConstructor]
        public ActionResolver(Guid id, DateTime created, CardAction action, Guid? fromCardId, Guid? toCardId, Guid userId)
        {
            Id = id;
            Created = created;
            Action = action;
            FromCardId = fromCardId;
            ToCardId = toCardId;
            UserId = userId;
        }

        public ActionResolver(CardAction action, Guid? fromCardId, Guid? toCardId, Guid userId)
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
            Action = action;
            FromCardId = fromCardId;
            ToCardId = toCardId;
            UserId = userId;
        }
    }
}
