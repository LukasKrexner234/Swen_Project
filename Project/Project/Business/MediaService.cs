using Project.DataAccess;
using Project.Domain;

namespace Project.Business;

public class MediaService
{
    private readonly MediaRepository _repository;

    public MediaService()
    {
        _repository = new MediaRepository();
    }

    // CREATE
    public async Task<int> CreateMediaAsync(MediaEntry media)
    {
        ValidateMedia(media);

        if (media.CreatedBy <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }

        return await _repository.CreateMediaAsync(media);
    }

    // READ: Alle Medien
    public async Task<List<MediaEntry>> GetAllMediaAsync()
    {
        return await _repository.GetAllMediaAsync();
    }

    // READ: Einzelnes Medium
    public async Task<MediaEntry?> GetMediaByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Invalid media ID");
        }

        return await _repository.GetMediaByIdAsync(id);
    }

    // UPDATE
    public async Task<bool> UpdateMediaAsync(MediaEntry media)
    {
        if (media.Id <= 0)
        {
            throw new ArgumentException("Invalid media ID");
        }

        if (media.CreatedBy <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }

        ValidateMedia(media);

        return await _repository.UpdateMediaAsync(media);
    }

    // DELETE
    public async Task<bool> DeleteMediaAsync(int id, int userId)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Invalid media ID");
        }

        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }

        return await _repository.DeleteMediaAsync(id, userId);
    }

    // Validierung
    private static void ValidateMedia(MediaEntry media)
    {
        if (string.IsNullOrWhiteSpace(media.Title))
        {
            throw new ArgumentException("Title is required");
        }

        if (media.Title.Length > 255)
        {
            throw new ArgumentException("Title is too long");
        }

        if (string.IsNullOrWhiteSpace(media.MediaType))
        {
            throw new ArgumentException("Media type is required");
        }

        if (media.MediaType.Length > 50)
        {
            throw new ArgumentException("Media type is too long");
        }

        if (media.ReleaseYear < 1900)
        {
            throw new ArgumentException("Invalid release year");
        }

        if (media.Genre.Length > 100)
        {
            throw new ArgumentException("Genre is too long");
        }

        if (media.AgeRestriction < 0 || media.AgeRestriction > 18)
        {
            throw new ArgumentException("Invalid age restriction");
        }
    }
}