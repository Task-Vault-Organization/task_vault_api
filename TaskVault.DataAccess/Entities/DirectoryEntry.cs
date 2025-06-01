namespace TaskVault.DataAccess.Entities;

public class DirectoryEntry
{
    public Guid UserId { get; set; }
    public Guid DirectoryId { get; set; }
    public Guid FileId { get; set; }
    public int Index { get; set; }

    public virtual User? User { get; set; }
    public virtual File? Directory { get; set; }
    public virtual File? File { get; set; }
}