using System.ComponentModel.DataAnnotations;

namespace LibraryData.Models
{
    public class Video : LibraryAssets
    {
        [Required]
        public string Director { get; set; }
    }
}
