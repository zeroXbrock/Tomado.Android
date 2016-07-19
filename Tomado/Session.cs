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
		double start, end;
		string title;

		public Session(double start, double end, string title) {
			this.start = start;
			this.end = end;
			this.title = title;
		}

		public double Start {
			get { return start; }
			set { start = value; }
		}
		public double End {
			get { return end; }
			set { end = value; }
		}
		public string Title {
			get { return title; }
		}
	}
}