using Chirp.Core.Models;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Infrastructure.Service;

public class CheepService : ICheepService
{
    private readonly ICheepRepository _cheepRepo;
    private readonly IAuthorRepository _authorRepo;

    public CheepService(ICheepRepository cheepRepo, IAuthorRepository authorRepo)
    {
        _cheepRepo = cheepRepo;
        _authorRepo = authorRepo;
    }

    // Get list of author ids that the user is following
    private async Task<List<int>> GetFollowedIdsAsync(string? currentAuthor)
    {
        if (string.IsNullOrEmpty(currentAuthor))
            return new List<int>();

        return await _authorRepo.GetFollowedAuthorIdsAsync(currentAuthor);
    }
    
    // Get all cheeps on public timeline - we use this one for unauthorized users
    public List<CheepViewModel> GetCheeps(int page = 0, int pageSize = 32)
        => GetCheeps(null, page, pageSize);

    // Get all cheeps from the authors we follow - used on following timeline
    public List<CheepViewModel> GetCheeps(string? currentAuthor, int page = 0, int pageSize = 32)
    {
        var followedIds = GetFollowedIdsAsync(currentAuthor).Result;
        var cheepDTOs = _cheepRepo.ReadCheepsAsync(author: null, page: page, pageSize: pageSize).Result;

        return cheepDTOs.Select(dto => new CheepViewModel(
            dto.Id,
            dto.Author,
            dto.Text,
            dto.Timestamp ?? string.Empty,
            dto.CommentCount,
            followedIds.Contains(dto.AuthorId)
        )).ToList();
    }

    // Get all cheeps from a specific author - we use this for authorTimeline
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 0, int pageSize = 32)
        => GetCheepsFromAuthor(author, null, page, pageSize);

    public List<CheepViewModel> GetCheepsFromAuthor(
        string author,
        string? currentAuthor,
        int page = 0,
        int pageSize = 32)
    {
        var followedIds = GetFollowedIdsAsync(currentAuthor).Result;
        var cheepDTOs = _cheepRepo.ReadCheepsAsync(author: author, page: page, pageSize: pageSize).Result;

        return cheepDTOs.Select(dto => new CheepViewModel(
            dto.Id,
            dto.Author,
            dto.Text,
            dto.Timestamp ?? string.Empty,
            dto.CommentCount,
            followedIds.Contains(dto.AuthorId)
        )).ToList();
    }

    // Create cheep - we call the cheepRepository to create a cheep
    public async Task CreateCheepAsync(string authorName, string text)
    {
        var dto = new CheepDTO
        {
            Author = authorName,
            Text = text
        };

        await _cheepRepo.CreateCheepAsync(dto);
    }
    
    // Get all cheeps from the authors we follow - we call the cheepRepo to get cheeps
    public List<CheepViewModel> GetCheepsFromFollowing(string currentAuthor, int page = 0, int pageSize = 32)
    {
        var followedIds = GetFollowedIdsAsync(currentAuthor).Result;
        if (followedIds.Count == 0)
            return new List<CheepViewModel>();

        var cheepDTOs = _cheepRepo.ReadCheepsFromAuthorIdsAsync(followedIds, page, pageSize).Result;

        return cheepDTOs.Select(dto => new CheepViewModel(
            dto.Id,
            dto.Author,
            dto.Text,
            dto.Timestamp ?? string.Empty,
            dto.CommentCount,
            true
        )).ToList();
    }
}
