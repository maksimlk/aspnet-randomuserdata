using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RandomUserData.Data.Enums
{
	public enum Regions
	{
		[Display(Name = "English")]
		en = 0,
		[Display(Name = "Poland")]
		pl = 1,
		[Display(Name = "Georgia")]
		ge = 2,
		[Display(Name = "France")]
		fr = 3,
		[Display(Name = "Japan")]
		ja = 4,
		[Display(Name = "Russia")]
		ru = 5
	}
}
