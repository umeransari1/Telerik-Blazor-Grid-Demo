using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelerikGridWithMongoDBAndSQL.Data
{
    public class People
    {
        [Key]
        public int id { get; set; }

        [MaxLength(100)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [MaxLength(100)]
        [DisplayName("Last Name")]
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [Column(TypeName = "Date")]
        [DisplayName("Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(400)]
        [DisplayName("Email Address")]
        public string EmailAddress { get; set; }
    }
}
