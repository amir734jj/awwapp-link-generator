using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Interfaces
{
    public interface IAwwAppLogic
    {
        Task<List<string>> GenerateLinks(int count, bool cacheMode = false);
    }
}