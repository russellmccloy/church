using church_api.ViewModels;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class PrayerCardViewModel : DocumentBaseViewModel
{
    public required string Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public required string Description { get; set; }

    public required string ImagePath { get; set; }

    public required Guid UserId { get; set; }
}
