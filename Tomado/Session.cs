using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

namespace Tomado {
	/// <summary>
	/// Session class; holds all info about a session.
	/// </summary>
	public class Session {
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }

		int startHour, startMinute, year, monthOfYear, dayOfMonth;
		string title;

		public Session(int ID, int startHour, int startMinute, int year, int monthOfYear, int dayOfMonth, string title) {
			this.ID = ID;
			StartHour = startHour;
			StartMinute = startMinute;
			Year = year;
			MonthOfYear = monthOfYear;
			DayOfMonth = dayOfMonth;
			Title = title;
		}

		public Session(int ID, DateTime dateTime, string title) {
			this.ID = ID;
			StartHour = dateTime.Hour;
			StartMinute = dateTime.Minute;
			Year = dateTime.Year;
			MonthOfYear = dateTime.Month;
			DayOfMonth = dateTime.Day;
			Title = title;
		}

		public Session() { }

		public int StartHour {
			get { return startHour; }
			set { startHour = value; }
		}
		public int StartMinute {
			get { return startMinute; }
			set { startMinute = value; }
		}
		public int Year {
			get { return year; }
			set { year = value; }
		}
		public int MonthOfYear {
			get { return monthOfYear; }
			set { monthOfYear = value; }
		}
		public int DayOfMonth {
			get { return dayOfMonth; }
			set { dayOfMonth = value; }
		}
		public string Title {
			get { return title; }
			set { title = value; }
		}
	}
}