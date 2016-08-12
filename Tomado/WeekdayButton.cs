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
	public class WeekdayButton {
		public WeekdayButton(Button button, DayOfWeek dayOfWeek, bool toggled = false) {
			Button = button;
			Toggled = toggled;
			DayOfWeek = dayOfWeek;

			button.Click += delegate {
				button.SetBackgroundResource(toggled ? Resource.Color.base_app_color : Resource.Color.base_app_complementary_color);
				toggled = !toggled;
			};
		}

		//0-indexed!
		public DayOfWeek DayOfWeek {
			get;
			set;
		}

		public Button Button {
			get;
			set;
		}

		public bool Toggled {
			get;
			set;
		}
	}
}