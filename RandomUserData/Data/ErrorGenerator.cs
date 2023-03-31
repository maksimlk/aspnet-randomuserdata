using Bogus;
using RandomUserData.Data.Enums;
using RandomUserData.Models;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace RandomUserData.Data
{
	public class ErrorGenerator
	{
		private readonly Random _random;
		private const double ERRORS_NUMBERS_TO_CHARS_RATIO = 0.25;
		private readonly List<string> recordProperties = new List<string>{ "Name", "Address", "Phone" };
		public ErrorGenerator(int seed)
		{
			_random = new Random(seed);
		}

		public void AddErrorsToRecord(ref RandomRecordModel record, double numberOfErrors)
		{
			if(record == null)
			{
				throw new ArgumentNullException("Cannot add errors to the record! The record passed is null");
			}
			for(int i = 0; i < numberOfErrors; i++)
			{
				var errorType = (DataErrorType)_random.Next(4);
				AddErrorToRecord(ref record, errorType);
			}
			if(_random.NextDouble() < numberOfErrors - (int)numberOfErrors) 
			{
				var errorType = (DataErrorType)_random.Next(4);
				AddErrorToRecord(ref record, errorType);
			}

		}

		private void AddErrorToRecord(ref RandomRecordModel record, DataErrorType errorType) 
		{
			switch(errorType)
			{
				case DataErrorType.MissingChar:
					DeleteChar(ref record); break;
				case DataErrorType.AdditionalChar:
					AddRandomChar(ref record); break;
				case DataErrorType.SwappedChar:
					SwapTwoChars(ref record); break;
			}

		}

		private void DeleteChar(ref RandomRecordModel record)
		{
			var field = GetRandomField();
			string temp = (string)field.GetValue(record);
			if (temp.Length == 0)
				return;
			temp = temp.Remove(_random.Next(temp.Length), 1);
			field.SetValue(record, temp);
		}

		private void AddRandomChar(ref RandomRecordModel record)
		{
			var field = GetRandomField();
			string temp = (string)field.GetValue(record);

			string toInsert;
			if(_random.NextDouble() < ERRORS_NUMBERS_TO_CHARS_RATIO)
			{
				toInsert = _random.Next(10).ToString();
			}
			else
			{
				var locale = record.RecordLocale;
				var lorem = new Bogus.DataSets.Lorem(locale: locale.ToString());
				toInsert = lorem.Letter(1);
			}
			temp = temp.Insert(_random.Next(temp.Length), toInsert);
			field.SetValue(record, temp);
		}

		private void SwapTwoChars(ref RandomRecordModel record)
		{
			var field = GetRandomField();
			string temp = (string)field.GetValue(record);
			if (temp.Length == 0) return;
			var newString = new StringBuilder(temp);
			var pos1 = _random.Next(temp.Length);
			var pos2 = _random.Next(temp.Length);
			(newString[pos1], newString[pos2]) = (newString[pos2], newString[pos1]);
			field.SetValue(record, newString.ToString());
		}

		private PropertyInfo GetRandomField()
		{
			Type type = typeof(RandomRecordModel);
			string propertyName = recordProperties[_random.Next(recordProperties.Count)];
			PropertyInfo property = type.GetProperty(propertyName);
			if (property == null)
				throw new ArgumentNullException("field " + propertyName + " does not exist in " + type.ToString());
			return property;
		}
	}
}
