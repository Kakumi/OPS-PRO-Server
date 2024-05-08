using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class ValueModifier
    {
        public ModifierDuration Duration { get; }
        public int Value { get; }

        public ValueModifier(ModifierDuration duration, int value)
        {
            Duration = duration;
            Value = value;
        }
    }
}
