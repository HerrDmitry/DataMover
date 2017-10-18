using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Writers
{
	public static partial class Writers
	{
		public static StreamWriter GetWaveStreamWriter(this IFile fileConfig, Interfaces.ILog log)
		{
			return new StreamWriter(new WaveStream(fileConfig,log),Encoding.UTF8);
		}

		private static Func<string> GetMetadataBuilder(this IFile fileConfig)
		{
			if (!(fileConfig.Rows?.Count > 0))
			{
				throw new ArgumentOutOfRangeException(Localization.GetLocalizationString("Missing row configuration"));
			}
			if (fileConfig.Rows.Count > 1)
			{
				throw new ArgumentOutOfRangeException(
					Localization.GetLocalizationString("Wave analytics does not support csv with variable rows"));
			}

			if (!(fileConfig.Rows[0].Columns?.Count > 0))
			{
				throw new ArgumentOutOfRangeException(Localization.GetLocalizationString("Missing columns configuration"));
			}
			var xmd = new XMD();
			xmd.FileFormat=new XMD.XMDFileFormat{CharsetName = "UTF-8", FieldsDelimitedBy = ",", FieldsEnclosedBy = "\"", NumberOfLinesToIgnore = 0};
			xmd.Objects = new List<XMD.XMDObject>
			{
				new XMD.XMDObject
				{
					FullyQualifiedName = fileConfig.Name.Replace(" ","_"),
					Label = fileConfig.Name,
					Name = fileConfig.Name.Replace(" ","_"),
					Fields =
						fileConfig.Rows[0].Columns.Select(c =>
						{
							var field = new XMD.XMDField();
							switch (c.Type)
							{
								case ColumnType.Date:
									field.Type = XMD.XMDFieldType.Date;
									break;
								case ColumnType.Decimal:
								case ColumnType.Integer:
								case ColumnType.Money:
									field.Type = XMD.XMDFieldType.Numeric;
									break;
								default:
									field.Type = XMD.XMDFieldType.Text;
									break;
							}
							field.Name = c.Name.Replace(" ", "_");
							field.FullyQualifiedName = fileConfig.Name.Replace(" ", "_") + "." + field.Name;
							field.Label = c.Name;
							field.Description = c.Description;
							if (field.Type == XMD.XMDFieldType.Numeric)
							{
								field.DefaultValue = "0";
								field.Precision = 18;
								field.Scale = 2;
							}
							if (field.Type == XMD.XMDFieldType.Date)
							{
								field.Format = c.Format ?? "yyyy-MM-dd HH:mm:ss";
							}

							return field;
						}).ToList()
				}
			};
			var xmdStr = JsonConvert.SerializeObject(xmd,new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});
			return () => xmdStr;
		}

		private static async Task<Func<MemoryStream, bool, int>> GetDataSenderFunc(this IFile fileConfig, Interfaces.ILog log)
		{
			var chunkNumber = 0;
			var queue = new ConcurrentQueue<MemoryStream>();
			var isFinalizing = false;
			var uploader = await fileConfig.GetDataChunkUploader(log);
			var senderTask=new Task(() =>
			{
				while (true)
				{
					while (queue.TryDequeue(out var nextChunk))
					{
						try
						{
							chunkNumber = uploader(nextChunk, isFinalizing && queue.IsEmpty);
						}
						catch
						{
							log.Fatal(Localization.GetLocalizationString("Upload has terminated"));
							return;
						}
					}
					if (isFinalizing && queue.IsEmpty)
					{
						return;
					}
					Thread.Sleep(100);
				}
			});
			senderTask.Start();
			return (stream, finalize) =>
			{
				if (stream != null)
				{
					queue.Enqueue(stream);
					while (queue.Count > 3)
					{
						Thread.Sleep(50);
					}
				}
				isFinalizing = finalize;
				if (isFinalizing)
				{
					log.Info(Localization.GetLocalizationString("Awaiting for upload to finish."));
					senderTask.Wait();
				}

				return chunkNumber;
			};
		}

		private static async Task InitiateDatasetUpload(this WaveContext context, string xmdJson, Interfaces.ILog log)
		{
			var url = $"{context.EntryPoint}/services/data/v41.0/sobjects/InsightsExternalData";
			log.Info(Localization.GetLocalizationString("Initializing data upload"));
			var client = new HttpClient();
			var encJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmdJson));
			var payload =
				$"{{\"Format\":\"csv\",\"EdgemartAlias\":\"{context.Alias}\",\"Operation\":\"Append\",\"Action\":\"None\",\"MetadataJson\":\"{encJson}\"}}";
			var content = new StringContent(payload,Encoding.ASCII);
			content.Headers.ContentType=new MediaTypeHeaderValue("application/json");
			client.AddAuthorization(context);
			var response = await client.PostAsync(url, content);
			var responseType = new {id = ""};
			var responseText = await response.Content.ReadAsStringAsync();
			log.Debug($"Received {responseText}");
			if (response.IsSuccessStatusCode)
			{
				context.SetId = JsonConvert.DeserializeAnonymousType(responseText, responseType).id;
				log.Info(Localization.GetLocalizationString("Received job id {0}", context.SetId));
			}
		}

		private static async Task FinalizeDatasetUpload(this WaveContext context,Interfaces.ILog log)
		{
			var client=new HttpClient();
			var url = $"{context.EntryPoint}/services/data/v36.0/sobjects/InsightsExternalData/{context.SetId}";
			log.Info(Localization.GetLocalizationString("Finalizing upload job {0}", context.SetId));
			var content=new StringContent("{\"Action\" : \"Process\"}",Encoding.ASCII);
			content.Headers.ContentType=new MediaTypeHeaderValue("application/json");
			client.AddAuthorization(context);
			var response = await client.PatchAsync(url, content);
			if (response.IsSuccessStatusCode)
			{
				log.Info(Localization.GetLocalizationString("Successfully uploaded {0}", context.Alias));
			}
			throw new ImporterUploadException(await response.Content.ReadAsStringAsync());
		}

		private static async Task<Func<MemoryStream,bool,int>> GetDataChunkUploader(this IFile fileConfig, Interfaces.ILog log)
		{
			var getContext = await fileConfig.GetWaveContextFunc(log);
			var xmdJson = fileConfig.GetMetadataBuilder();
			var chunkNo = 0;
			return (stream, isFinalizing) =>
			{
				var context = getContext();
				if (string.IsNullOrWhiteSpace(context.SetId))
				{
					context.InitiateDatasetUpload(xmdJson(), log).Wait();
					if (string.IsNullOrWhiteSpace(context.SetId))
					{
						throw new ImporterException(
							Localization.GetLocalizationString("Could not get job id from wave, cannot upload chunks"));
					}
				}
				stream.Flush();
				var encCSVchunk = Convert.ToBase64String(stream.ToArray());
				var client = new HttpClient();
				var payload =
					$"{{\"InsightsExternalDataId\":\"{context.SetId}\",\"PartNumber\":{++chunkNo},\"DataFile\":\"{encCSVchunk}\"}}";
				var content = new StringContent(payload, Encoding.ASCII);
				content.Headers.ContentType=new MediaTypeHeaderValue("application/json");
				var url = $"{context.EntryPoint}/services/data/v41.0/sobjects/InsightsExternalDataPart";
				client.AddAuthorization(context);
				log.Debug($"Uploading chunk {chunkNo}");
				log.Debug(payload);
				var response = client.PostAsync(url, content).Result;
				if (response.IsSuccessStatusCode)
				{
					log.Debug($"Uploaded chunk #{chunkNo}");
					if (isFinalizing)
					{
						context.FinalizeDatasetUpload(log).Wait();
					}
					return chunkNo;
				}
				throw new ImporterUploadException(response.Content.ReadAsStringAsync().Result);
			};
		}

		private static void AddAuthorization(this HttpClient client, WaveContext context)
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Token);

		}

		private static async Task<Func<WaveContext>> GetWaveContextFunc(this IFile fileConfig,Interfaces.ILog log)
		{
			var client = new HttpClient();
			var url=$"{fileConfig.Path}/services/oauth2/token?grant_type=password&client_id={fileConfig.ClientId}&client_secret={fileConfig.ClientSecret}&username={fileConfig.Login}&password={fileConfig.Password}{fileConfig.Token}";
			var content = new StringContent(string.Empty);
			content.Headers.ContentType=new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			log.Info(string.Format(
				Localization.GetLocalizationString("Obtaining access token and entry point from \"{0}\" for \"{1}\""),
				fileConfig.Path, fileConfig.Login));			
			var response = await client.PostAsync(url, content);
			WaveContext context = null;
			if (response.IsSuccessStatusCode)
			{
				log.Info(Localization.GetLocalizationString("Retrieved entry point and access token."));
				var responseType = new {access_token = "", instance_url = ""};
				var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(),responseType);
				context = new WaveContext
				{
					Token = result.access_token,
					EntryPoint = result.instance_url,
					Alias = fileConfig.Name.Replace(" ","_")
				};
			}
			else
			{
				log.Fatal($"{response.StatusCode.ToString()}-{response.Content.ReadAsStringAsync()}");
				throw new UnauthorizedAccessException(Localization.GetLocalizationString("Could not get access to wave entry point."));
			}
			return () => context;
		}

		public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent iContent) {
			var method = new HttpMethod("PATCH");

			var request = new HttpRequestMessage(method, requestUri) {
				Content = iContent
			};

			return await client.SendAsync(request);
		}
		private class WaveContext
		{
			public string Token { get; set; }
			public string EntryPoint { get; set; }
			public string SetId { get; set; }
			public string Alias { get; set; }
		}
	}
}