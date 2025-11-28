namespace Chirp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Chirp.Core.Models;

public interface ICommentRepository
{
    Task<IEnumerable<CommentDTO>> GetCommentsByCheepIdAsync(int cheepId);
    Task<int> GetCommentCountByCheepIdAsync(int cheepId);
    Task CreateCommentAsync(CommentDTO comment);
    Task DeleteCommentAsync(int commentId);
}
