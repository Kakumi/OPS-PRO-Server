using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class TagModifier
    {
        public ModifierDuration Duration { get; }
        public string Value { get; }

        public TagModifier(ModifierDuration duration, string value)
        {
            Duration = duration;
            Value = value;
        }
    }
}
