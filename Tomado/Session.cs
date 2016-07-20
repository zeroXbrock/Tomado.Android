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
		int startHour, endHour, startMinute, endMinute;
		string title;

		public Session(int startHour, int startMinute, int endHour, int endMinute, string title) {
			this.startHour = startHour;
			this.startMinute = startMinute;
			this.endHour = endHour;
			this.endMinute = endMinute;
			this.title = title;
		}

		public int StartHour {
			get { return startHour; }
			set { startHour = value; }
		}
		public int StartMinute {
			get { return startMinute; }
			set { startMinute = value; }
		}
		public int EndHour {
			get { return endHour; }
			set { endHour = value; }
		}
		public int EndMinute {
			get { return endMinute; }
			set { endMinute = value; }
		}
		public string Title {
			get { return title; }
		}
	}
}