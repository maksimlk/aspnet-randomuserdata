using RandomUserData.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace RandomUserData.Models
{
	public class RandomRecordModel
	{
		public Regions RecordLocale;
		[Display(Name = "ID")]
		public int Id { get; set; }
		[Display(Name = "Random Identifier")]
		public string RandomIdentificator { get; set; }
		[Display(Name = "Full Name")]
		public string Name { get; set; }
		[Display(Name = "Full Address")]
		public string Address { get; set; }
		[Display(Name = "Phone Number")]
		public string Phone { get; set; }
	}
}
