using System;

namespace LibTicker.Clients
{
    internal struct Request
    {
        internal Guid Subject;
        internal Guid Trigger;
        internal string Data;
    }
}