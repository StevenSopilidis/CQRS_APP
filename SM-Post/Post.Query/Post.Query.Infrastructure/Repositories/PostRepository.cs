using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DatabaseContextFactory _factory;

        public PostRepository(DatabaseContextFactory factory)
        {
            _factory = factory;
        }

        public async Task CreateAsync(PostEntity post)
        {
            using var context = _factory.CreateDbContext();
            context.Posts.Add(post);
            _ = await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid postId)
        {
            using var context = _factory.CreateDbContext();
            var post = await GetByIdAsync(postId);
            if (post is null)
                return;

            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }

        public async Task<PostEntity?> GetByIdAsync(Guid postId)
        {
            using var context = _factory.CreateDbContext();
            return await context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<List<PostEntity>> ListAllAsync()
        {
            using var context = _factory.CreateDbContext();
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListByAuthorAsync(string author)
        {
            using var context = _factory.CreateDbContext();
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .AsNoTracking()
                .Where(p => p.Author == author)
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListWithCommentsAsync()
        {
            using var context = _factory.CreateDbContext();
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .AsNoTracking()
                .Where(p => p.Comments != null && p.Comments.Any())
                .ToListAsync();
        }

        public async Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes)
        {
            using var context = _factory.CreateDbContext();
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.Comments)
                .AsNoTracking()
                .Where(p => p.Likes >= numberOfLikes)
                .ToListAsync();
        }

        public async Task UpdateAsync(PostEntity post)
        {
            using var context = _factory.CreateDbContext();
            context.Posts.Update(post);

            _ = await context.SaveChangesAsync();
        }
    }
}