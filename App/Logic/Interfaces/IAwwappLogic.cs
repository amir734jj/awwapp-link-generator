using System.Collections.Generic;

namespace App.Logic.Interfaces
{
    public interface IAwwAppLogic
    {
        IAsyncEnumerable<string> GenerateLinks(int count);
    }
}