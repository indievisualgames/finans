using System;

namespace Game.Core
{
    public interface ICalculatorInputSource
    {
        event Action<string> OnKey;
    }
}


