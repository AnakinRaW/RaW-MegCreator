using System;

namespace MegCreatorCLI
{
    public interface IPacker : IDisposable
    {
        void Pack();
    }
}