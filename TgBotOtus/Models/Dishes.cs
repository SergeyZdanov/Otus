using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgBotOtus.Models
{
    public class Dishes
    {
        public int ID { get; set; }
        public int DietaNameId { get; set; }
       // [ForeignKey("DietaName")]
        public string DishName { get; set; }
        public string Ingredients { get; set; }
        public string Recept { get; set; }
       // public DietName DietName { get; set; }
    }
}
