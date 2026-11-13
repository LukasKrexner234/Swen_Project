namespace Project.Domain;

public class MediaEntry
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string Genre { get; set; } = string.Empty;

    public int AgeRestriction { get; set; }

    public int CreatedBy { get; set; }
}