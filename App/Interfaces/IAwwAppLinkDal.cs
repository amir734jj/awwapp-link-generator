using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Interfaces
{
    public interface IAwwAppLinkDal
    {
        public Task MarkUsed(string link);
        
        public Task Insert(string link);

        public Task<List<string>> Collect(int count);

        Task Clean();
    }
}