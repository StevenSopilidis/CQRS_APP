using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;

namespace Post.Query.Api.Queries
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IPostRepository _postRepo;

        public QueryHandler(IPostRepository postRepo)
        {
            _postRepo = postRepo;
        }

        public async Task<List<PostEntity>> HandleAsync(FindAllPostsQuery query)
        {
            return await _postRepo.ListAllAsync();
        }

        public async Task<List<PostEntity>> HandleAsync(FindPostByIdQuery query)
        {
            var post = await _postRepo.GetByIdAsync(query.Id);
            return new List<PostEntity>{ post };
        }

        public async Task<List<PostEntity>> HandleAsync(FindPostsByAuthorQuery query)
        {
            return await _postRepo.ListByAuthorAsync(query.Author);
        }

        public async Task<List<PostEntity>> HandleAsync(FindPostsWithCommentsQuery query)
        {
            return await _postRepo.ListWithCommentsAsync();
        }

        public async Task<List<PostEntity>> HandleAsync(FindPostsWithLikesQuery query)
        {
            return await _postRepo.ListWithLikesAsync(query.NumberOfLikes);
        }
    }
}