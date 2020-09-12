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

        public AwwAppLinkDal(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
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

            foreach (var model in (await session.Query<AwwAppLink>().ToListAsync()).Where(x => DateTimeOffset.Now.Subtract(x.CreatedOn) < TimeSpan.FromDays(7)))
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
                .ToListAsync()).Where(x => DateTimeOffset.Now.Subtract(x.CreatedOn) < TimeSpan.FromDays(7)).ToList();

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