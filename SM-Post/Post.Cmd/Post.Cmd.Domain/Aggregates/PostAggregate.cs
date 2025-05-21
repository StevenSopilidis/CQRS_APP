using CQRS.Core.Domain;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate : AggregateRoot
    {
        private bool _active;
        private string _author;
        private readonly Dictionary<Guid, Tuple<string, string>>_comments = new();
        
        public bool Active {
            get => _active;
            set => _active = value;
        }

        public PostAggregate() {}
        public PostAggregate(Guid id, string author, string message) {
            RaiseEvent(new PostCreatedEvent{
                Id = id,
                Author = author,
                Message = message,
                DatePosted = DateTime.Now
            });
        }

        public void Apply(PostCreatedEvent @event) {
            _id = @event.Id;
            _active = true;
            _author = @event.Author;
        }
        
        public void LikePost() {
            if (!_active)
                throw new InvalidOperationException("You cannot like inactive post");

            RaiseEvent(new PostLikedEvent{
                Id= _id
            });
        }
        public void Apply(PostLikedEvent @event) {
            _id = @event.Id;
        }


        public void EditMessage(string message) {
            if (!_active)
                throw new InvalidOperationException("You cannot edit inactive post");

            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException($"The value of {nameof(message)} cannot be null or empty");

            RaiseEvent(new MessageUpdatedEvent{
                Id = _id,
                Message = message
            });
        }
        public void Apply(MessageUpdatedEvent @event) {
            _id = @event.Id;
        }
    
        public void AddComment(string comment, string username) {
            if (!_active)
                throw new InvalidOperationException("You cannot add comment to an inactive post");
        
            if (string.IsNullOrWhiteSpace(comment))
                throw new InvalidOperationException($"The value of {nameof(comment)} cannot be null or empty");

            RaiseEvent(new CommentAddedEvent{
                Id= _id,
                CommentId= Guid.NewGuid(),
                Comment= comment,
                CommentDate= DateTime.Now
            });
        }
        public void Apply(CommentAddedEvent @event) {
            _id = @event.Id;
            _comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }

        public void EditComment(Guid commentId, string comment, string username) {
            if (!_active)
                throw new InvalidOperationException("You cannot edit comment to an inactive post");

            if (!_comments.ContainsKey(commentId))
                throw new InvalidOperationException("You cannot edit invlid comment");

            if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                throw new InvalidOperationException("You do not own comment");

            RaiseEvent(new CommentUpdatedEvent{
                Id = _id,
                CommentId = commentId,
                Comment = comment,
                Username = username,
                EditDate = DateTime.Now
            });
        }
        public void Apply(CommentUpdatedEvent @event) {
            _id = @event.Id;
            _comments[@event.CommentId] = new(@event.Comment, @event.Username);
        }

        public void RemoveComment(Guid commentId, string username) {
            if (!_active)
                throw new InvalidOperationException("You cannot remove comment to an inactive post");

            if (!_comments.ContainsKey(commentId))
                throw new InvalidOperationException("You cannot remove invlid comment");

            if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                throw new InvalidOperationException("You do not own comment");

            RaiseEvent(new CommentRemovedEvent{
                Id = _id,
                CommentId = commentId
            });
        }
        public void Apply(CommentRemovedEvent @event) {
            _id = @event.Id;
            _comments.Remove(@event.CommentId);
        }

        public void DeletePost(string username) {
            if (!_active)
                throw new InvalidOperationException("Post has alread been removed");
            
            if (!_author.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                throw new InvalidOperationException("You do not own the post");
            
            RaiseEvent(new PostRemovedEvent{
                Id = _id,
            });
        }
        public void Apply(PostRemovedEvent @event) {
            _id = @event.Id;
            _active = false;
        }
    }            

}