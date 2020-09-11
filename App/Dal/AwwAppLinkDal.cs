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

            session.Store(new AwwAppLink {Link = link});

            await session.SaveChangesAsync();
        }

        public async Task MarkUsed(string link)
        {
            using var session = _documentStore.OpenSession();

            var model = await session.Query<AwwAppLink>().FirstOrDefaultAsync(x => x.Link == link);

            model.Used = true;

           session.Update(model);
           
           await session.SaveChangesAsync();
        }

        public async Task<List<string>> Collect(int count)
        {
            using var session = _documentStore.OpenSession();

            var models = await session.Query<AwwAppLink>()
                .Where(x => x.Used == false && DateTimeOffset.Now.Subtract(x.CreatedOn) < TimeSpan.FromDays(7))
                .Take(count)
                .ToListAsync();

            foreach (var model in models)
            {
                model.Used = true;
            }
            
            session.Update(models);

            await session.SaveChangesAsync();

            return models.Select(x => x.Link).ToHashSet().ToList();
        }
    }
}