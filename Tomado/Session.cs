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

namespace Tomado {
	public class Session {
		int startHour, startMinute, year, monthOfYear, dayOfMonth;
		string title;

		public Session(int startHour, int startMinute, int year, int monthOfYear, int dayOfMonth, string title) {
			StartHour = startHour;
			StartMinute = startMinute;
			Year = year;
			MonthOfYear = monthOfYear;
			DayOfMonth = dayOfMonth;
			Title = title;
		}

		public Session(DateTime dateTime, string title) {
			StartHour = dateTime.Hour;
			StartMinute = dateTime.Minute;
			Year = dateTime.Year;
			MonthOfYear = dateTime.Month;
			DayOfMonth = dateTime.Day;
			Title = title;
		}

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