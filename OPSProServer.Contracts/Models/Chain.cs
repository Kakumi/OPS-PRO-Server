using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OPSProServer.Contracts.Models.FlowAction;

namespace OPSProServer.Contracts.Models
{
    public class Chain
    {
        public Guid Id { get; }
        public User FromUser { get; }
        public User ToUser { get; }
        public Guid? FromCardId { get; set; }
        public Guid? ToCardId { get; set; }
        public FlowAction? Action { get; private set; }
        public FlowContext Context { get; set; }
    }
}
