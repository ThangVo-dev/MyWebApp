using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApp.Shared.Models.User
{
    public class UserMdl
    {
        public string? Id { get; set; }
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required")]

        public string? FullName { get; set; }
        [Display(Name = "User Name")]
        public string? UserName { get; set; }
        public string? Language { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone number is required")]
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; }
        
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}