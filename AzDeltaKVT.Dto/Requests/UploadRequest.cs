using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzDeltaKVT.Dto.Requests
{
	public class UploadRequest
	{
		public IFormFile TsvFile { get; set; }
	}
}
