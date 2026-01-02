namespace Chirp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Chirp.Core.Models;

public class CommentRepository : ICommentRepository
{
    private readonly ChatDBContext _context;

    public CommentRepository(ChatDBContext context)
    {
        _context = context;
    }

    // Get comments through the id of the cheep they belong to
    public async Task<IEnumerable<CommentDTO>> GetCommentsByCheepIdAsync(int cheepId)
    {
        return await _context.Comments
            .Where(c => c.CheepId == cheepId)
            .Include(c => c.Author)
            .OrderBy(c => c.TimeStamp)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                Text = c.Text,
                TimeStamp = c.TimeStamp,
                AuthorName = c.Author.UserName,
                CheepId = c.CheepId
            })
            .ToListAsync();
    }

    // Get amount of comments on a cheep
    public async Task<int> GetCommentCountByCheepIdAsync(int cheepId)
    {
        return await _context.Comments
            .CountAsync(c => c.CheepId == cheepId);
    }

    // Create a comment
    public async Task CreateCommentAsync(CommentDTO comment)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Email == comment.AuthorName || a.UserName == comment.AuthorName);

        if (author == null)
        {
            author = new Author
            {
                UserName = comment.AuthorName,
                Email = comment.AuthorName
            };
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
        }

        var newComment = new Comment
        {
            Text = comment.Text,
            CheepId = comment.CheepId,
            AuthorId = author.Id,
            TimeStamp = DateTime.UtcNow
        };

        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();
    }

    // Delete a comment
    public async Task DeleteCommentAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
