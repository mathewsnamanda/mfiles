using Mfiles.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mfiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MfilesController : ControllerBase
    {
        [HttpGet]
        public IActionResult getmfiles([FromQuery] string Username, [FromQuery] string Password, [FromQuery] string phrase)
        {
			var str = "";
			if (!string.IsNullOrWhiteSpace(Username))
				Username = Username.Trim();
			if (!string.IsNullOrWhiteSpace(Password))
				Password = Password.Trim();

			var jsonSerializer = JsonSerializer.CreateDefault();
			var username = Username;
			var password = Password;
			// Create the authentication details.
			var auth = new
			{
				Username = Username,
				Password = Password,
				VaultGuid = "{80562C09-7172-435F-AE40-B4B1C7FB1D15}" // Use GUID format with {braces}.
			};

			var authenticationRequest = (HttpWebRequest)WebRequest.Create("http://66.23.226.169:81/REST/server/authenticationtokens.aspx");
			authenticationRequest.Method = "POST";
			using (var streamWriter = new StreamWriter(authenticationRequest.GetRequestStream()))
			{
				using (var jsonTextWriter = new JsonTextWriter(streamWriter))
				{
					jsonSerializer.Serialize(jsonTextWriter, auth);
				}
			}

			// Execute the request.
			var authenticationResponse = (HttpWebResponse)authenticationRequest.GetResponse();

			// Extract the authentication token.
			string authenticationToken = null;
			using (var streamReader = new StreamReader(authenticationResponse.GetResponseStream()))
			{
				using (var jsonTextReader = new JsonTextReader(streamReader))
				{
					authenticationToken = ((dynamic)jsonSerializer.Deserialize(jsonTextReader)).Value;
				}
			}
			if (!string.IsNullOrWhiteSpace(phrase))
			{
				phrase = phrase.Trim();
				str += "&" + "p0*" + "=" + phrase;
			}
			var client = new RestClient($"http://66.23.226.169:81/REST/objects.aspx?0=0{str}");
			client.Timeout = -1;
			var request = new RestRequest(Method.GET);
			request.AddHeader("X-Authentication", $"{authenticationToken}");
			IRestResponse response = client.Execute(request);
			Example account = JsonConvert.DeserializeObject<Example>(response.Content);
			foreach (var item in account.Items)
			{
				int kool = 0;
				foreach (var item1 in item.Files)
                {
					kool = item1.ID;
				}
				item.Fileurl = $"http://66.23.226.169:81/REST /objects/{item.ObjVer.Type}/{item.ObjVer.ID}/{item.ObjVer.Version}/files/{kool}/content?Username={username}&Password={password}";
			}
			var commands = new List<Example> { account };
			return new JsonResult(commands);
		}
        }
}
