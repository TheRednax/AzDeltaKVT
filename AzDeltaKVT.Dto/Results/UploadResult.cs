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
		List<UploadRowResult> Rows { get; set; }
		IFormFile TsvFile { get; set; }
		public List<string> Errors { get; set; }
	}
}
