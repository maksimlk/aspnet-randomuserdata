using Bogus;
using RandomUserData.Data.Enums;
using RandomUserData.Models;

namespace RandomUserData.Data
{
	public class RandomRecordGenerator
	{
		private Faker<RandomRecordModel> faker;
		private ErrorGenerator errorGenerator;
		private int currentID;
		private Regions _locale;
		private int _seed;
		private double _numberOfErrors;
		public RandomRecordGenerator(Regions locale, int seed, double numberOfErrors)
		{
			_locale = locale;
			_numberOfErrors = numberOfErrors;
			_seed = seed;
			currentID = 1;
			CreateFaker();
			errorGenerator = new ErrorGenerator(_seed);
		}

		private void CreateFaker()
		{
			faker = new Faker<RandomRecordModel>(_locale.ToString())
				.RuleFor(u => u.RandomIdentificator, f => f.Random.Guid().ToString())
				.RuleFor(u => u.Name, f => f.Person.FullName)
				.RuleFor(u => u.Address, f => f.PickRandom(
				f.Address.City() + ", " + f.Address.StreetAddress(false),
				f.Address.State() + ", " + f.Address.City() + ", " + f.Address.StreetName() + ", " + f.Address.BuildingNumber()))
				.RuleFor(u => u.Phone, f => f.Phone.PhoneNumber());
			faker.UseSeed(_seed);
		}

		public Regions GetLocale() { return _locale; }
		public int GetSeed() { return _seed; }
		public double GetNumberOfErrors() { return _numberOfErrors; }

		public List<RandomRecordModel> GenerateFakeRecords(int numberOfRecords)
		{
			var randomRecords = new List<RandomRecordModel>();
			for (int i = 0; i < numberOfRecords; i++)
			{
				RandomRecordModel record = faker.Generate();
				record.Id = currentID++;
				record.RecordLocale = Enum.Parse<Regions>(faker.Locale);
				errorGenerator.AddErrorsToRecord(ref record, _numberOfErrors);
				randomRecords.Add(record);
			}
			return randomRecords;
		}
	}
}
