using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soe.WebApi.Models
{
    public class UpdateUserGaugeSettingsModel
    {
        [Required]
        public int UserGaugeId { get; set; }
        [Required]
        public List<UserGaugeSettingDTO> Settings { get; set; }
    }
}