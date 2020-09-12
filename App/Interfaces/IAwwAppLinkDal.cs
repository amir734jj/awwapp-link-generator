using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Interfaces
{
    public interface IAwwAppLinkDal
    {
        public Task Insert(string link);

        public Task<List<string>> Collect(int count);

        Task Clean();
    }
}