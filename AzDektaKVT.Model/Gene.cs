using System.ComponentModel.DataAnnotations;

namespace AzDektaKVT.Model
{
	public class Gene
	{
		[Key]
		public string Name { get; set; }

		public string Chromosome { get; set; }
		public int Start { get; set; }
		public int Stop { get; set; }
		public string UserInfo { get; set; }
	}

}
