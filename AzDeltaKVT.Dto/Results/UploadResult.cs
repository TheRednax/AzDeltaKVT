using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzDeltaKVT.Dto.Results
{
	public class UploadResult
	{
		public List<UploadRowResult> Rows { get; set; }
		public IFormFile TsvFile { get; set; }
		public List<string> Errors { get; set; }
	}
}
