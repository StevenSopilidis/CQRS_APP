using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContextFactory _factory;

        public CommentRepository(DatabaseContextFactory factory)
        {
            _factory = factory;
        }

        public async Task CreateAsync(CommentEntity comment)
        {
            using var context = _factory.CreateDbContext();
            context.Comments.Add(comment);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid commentId)
        {
            using var context = _factory.CreateDbContext();
            var comment = await GetByIdAsync(commentId);
            if (comment is null)
                return;

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();
        }

        public async Task<CommentEntity?> GetByIdAsync(Guid commentId)
        {
            using var context = _factory.CreateDbContext();
            return await context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);        
        }

        public async Task UpdateAsync(CommentEntity comment)
        {
            using var context = _factory.CreateDbContext();
            context.Comments.Update(comment);
            _ = await context.SaveChangesAsync();
        }
    }
}