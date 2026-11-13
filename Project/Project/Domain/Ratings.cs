namespace Project.Domain;

public class Ratings
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MediaEntryId { get; set; }

    public int Score { get; set; }

    public string Comment { get; set; } = string.Empty;
}