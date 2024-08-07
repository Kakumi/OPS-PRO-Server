﻿using System;

namespace OPSProServer.Contracts.Models.Scripts
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CardScriptAttribute : Attribute
    {
        public string Serie { get; }
        public string Number { get; }

        public CardScriptAttribute(string serie, string number)
        {
            Serie = serie;
            Number = number;
        }

        public string GetScriptCode()
        {
            return $"{Serie}-{Number}";
        }
    }
}
