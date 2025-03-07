using System.ComponentModel.DataAnnotations;

namespace MsaCookingApp.DataAccess.Entities;

public class UploadedFile
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }
}