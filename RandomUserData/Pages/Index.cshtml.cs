using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using RandomUserData.Data;
using RandomUserData.Data.Enums;
using RandomUserData.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace RandomUserData.Pages
{
	public class IndexModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public int cursor { get; set; } = 20;
		private RandomRecordGenerator recordGenerator;
		private readonly IMemoryCache _cache;
		private const int SEED_OFFSET = 155325;

		[ViewData]
		public List<RandomRecordModel> randomRecords { get; set; }

		[BindProperty(SupportsGet = true)]
		public InputModel Input { get; set; }
		public class InputModel
		{
			[Required(ErrorMessage = "Please choose the locale")]
			[DefaultValue(Regions.en)]
			public Regions Locale { get; set; }
			[Required(ErrorMessage = "Please specify the number of errors")]
			[DefaultValue(0)]
			[Range(0, 2000, ErrorMessage = "Error should be in the range [0; 2000]")]
			public double ErrorsNumber { get; set; }
			[Required(ErrorMessage = "Please provide the seed for the generator")]
			[DefaultValue(1)]
			[Range(0, 1e10, ErrorMessage= "Seed should be positive and contain maximum 9 digits!")]
			public int SeedValue { get; set; }
		}

		public IndexModel(IMemoryCache cache)
		{
			_cache = cache;
		}

		public IActionResult OnGet()
		{
			if (Input == null)
				Input = new InputModel();
			if (ModelState.IsValid)
			{
				_cache.Set("last_cursor_pos_csv_export", cursor);
				if (!Request.IsHtmx())
				{
					_cache.Remove("generator");
					recordGenerator = new RandomRecordGenerator(Input.Locale, GetSeedValue(Input.SeedValue, Input.Locale), Input.ErrorsNumber);
					randomRecords = recordGenerator.GenerateFakeRecords(20);
					_cache.Set<RandomRecordGenerator>("generator", recordGenerator);
					return Page();
				}
				if (_cache.TryGetValue("generator", out recordGenerator))
				{
					randomRecords = recordGenerator.GenerateFakeRecords(10);
					_cache.Set<RandomRecordGenerator>("generator", recordGenerator);
					return Partial("_RandomRecords", this);
				}
			}
			Input = new InputModel();
			return Page();
		}

		private int GetSeedValue(int seed, Regions region) {
			var complexSeed = (seed + 1) * ((int)region + 1) * SEED_OFFSET;
			return complexSeed;
		}

		/* This method could've been easily implemented on the client-side by the means of js as all the data is already there on the page.
		 * And it would reduce the load on the server. However, by implementing it on the server-side it could be then used
		 * for API calls. And I just wanted to implement it on c#
		 */
		public FileResult OnPostCSVExport()
		{
			FileStreamResult result = GetCSVResult(GenerateRandomRecords());
			return result;
		}

		private FileStreamResult GetCSVResult(List<RandomRecordModel> data)
		{
			var mem = new MemoryStream();
			var writer = new StreamWriter(mem, Encoding.UTF8);
			var csvWriter = new CsvWriter(writer, GetCsvConfiguration());
			csvWriter.WriteRecords(data);
			csvWriter.Flush();
			mem.Seek(0, SeekOrigin.Begin);
			FileStreamResult result = new FileStreamResult(mem, "text/csv");
			result.FileDownloadName = "random_data_exported.csv";
			return result;
		}

		private CsvConfiguration GetCsvConfiguration()
		{
			var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
			{
				HasHeaderRecord = true,
				Delimiter = ",",
				Encoding = Encoding.UTF8
			};
			return csvConfig;
		}

		private List<RandomRecordModel> GenerateRandomRecords()
		{
			int cursorpos = 1;
			RandomRecordGenerator? usedGenerator = null, newGenerator = null;
			var records = new List<RandomRecordModel>();
			if (_cache.TryGetValue("generator", out usedGenerator))
			{
				if(usedGenerator != null)
					newGenerator = new RandomRecordGenerator(usedGenerator.GetLocale(), usedGenerator.GetSeed(), usedGenerator.GetNumberOfErrors());
			}
			if (_cache.TryGetValue("last_cursor_pos_csv_export", out cursorpos))
			{
				if(newGenerator != null)
					records = newGenerator.GenerateFakeRecords(cursorpos);
			}
			return records;
		}
	}
}