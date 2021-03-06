using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Interfaces;
using App.Models;
using Marten;

namespace App.Dal
{
    public class AwwAppLinkDal : IAwwAppLinkDal
    {
        private readonly IDocumentStore _documentStore;
        
        private readonly TimeSpan _invalidateTimeSpan = TimeSpan.FromMinutes(15);

        public AwwAppLinkDal(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task MarkUsed(string link)
        {
            using var session = _documentStore.LightweightSession();

            var model = await session.Query<AwwAppLink>().FirstAsync(x => x.Link == link);

            model.Used = true;
            
            session.Update(model);

            await session.SaveChangesAsync();
        }

        public async Task Insert(string link)
        {
            using var session = _documentStore.LightweightSession();

            if (!session.Query<AwwAppLink>().Any(x => x.Link == link))
            {
                session.Store(new AwwAppLink {Link = link});
            }
            
            await session.SaveChangesAsync();
        }

        public async Task Clean()
        {
            using var session = _documentStore.LightweightSession();

            foreach (var model in (await session.Query<AwwAppLink>().ToListAsync()).Where(x => DateTimeOffset.Now.Subtract(x.CreatedOn) >= _invalidateTimeSpan))
            {
                session.Delete(model);
            }

            await session.SaveChangesAsync();
        }

        public async Task<List<string>> Collect(int count)
        {
            using var session = _documentStore.LightweightSession();

            var models = (await session.Query<AwwAppLink>()
                .Where(x => x.Used == false)
                .Take(count)
                .ToListAsync()).Where(x => DateTimeOffset.Now.Subtract(x.CreatedOn) < _invalidateTimeSpan).ToList();

            foreach (var model in models)
            {
                model.Used = true;
                
                session.Update(model);
            }
            
            await session.SaveChangesAsync();

            return models.Select(x => x.Link).ToHashSet().ToList();
        }
    }
}