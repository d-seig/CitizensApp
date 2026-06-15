using System.ComponentModel.DataAnnotations;
using DTO;

namespace Web.Models
{
    public class Citizen
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Необходимо ввести фамилию!")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Фамилия должна быть от {2} до {1} символов!")]
        public string Famil { get; set; } = "";

        [Required(ErrorMessage = "Необходимо ввести имя!")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Имя должно быть от {2} до {1} символов!")]
        public string Imja { get; set; } = "";

        [Required(ErrorMessage = "Необходимо ввести отчество!")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Отчество должно быть от {2} до {1} символов!")]
        public string Otch { get; set; } = "";

        [Required(ErrorMessage = "Необходимо ввести дату рождения!")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1753-01-01", "9999-12-31")]
        public DateTime? BirthDate { get; set; }

        public void Map(CitizenDTO citizenDTO)
        {
            Id = citizenDTO.Id;
            Famil = citizenDTO.Famil;
            Imja = citizenDTO.Imja;
            Otch = citizenDTO.Otch;
            BirthDate = citizenDTO.BirthDate;
        }
        public CitizenDTO Unmap(int id) => new()
        {
            Id = id,
            Famil = Famil,
            Imja = Imja,
            Otch = Otch,
            BirthDate = BirthDate
        };
    }
}
