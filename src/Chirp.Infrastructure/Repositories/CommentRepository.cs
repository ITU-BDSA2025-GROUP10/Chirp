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
                AuthorName = c.Author.Name,
                CheepId = c.CheepId
            })
            .ToListAsync();
    }

    public async Task<int> GetCommentCountByCheepIdAsync(int cheepId)
    {
        return await _context.Comments
            .CountAsync(c => c.CheepId == cheepId);
    }

    public async Task CreateCommentAsync(CommentDTO comment)
    {
        // Search by Email first (since AuthorName might contain email from OAuth)
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Email == comment.AuthorName || a.Name == comment.AuthorName);

        if (author == null)
        {
            // If author doesn't exist, create one (this matches the pattern in CheepRepository)
            author = new Author
            {
                Name = comment.AuthorName,
                Email = comment.AuthorName
            };
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
        }

        var newComment = new Comment
        {
            Text = comment.Text,
            CheepId = comment.CheepId,
            AuthorId = author.AuthorId,
            TimeStamp = DateTime.UtcNow
        };

        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();
    }

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
