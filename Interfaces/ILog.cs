using System;

namespace Interfaces
{
    public interface ILog
    {
        Action<string> Error { get; }
        Action<string> Warning { get; }
        Action<string> Info { get; }
        Action<string> Fatal { get; }
        Action<string> Debug { get; }
        Action Terminate { get; }
    }
}