using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Presentation.WebApi.Models;

public class ChangeAgentStatusRequest
{
    [Required]
    public bool? IsActive { get; set; }
}
