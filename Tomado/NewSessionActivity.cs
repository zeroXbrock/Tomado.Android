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
	[Activity(Label = "NewSessionActivity")]
	public class NewSessionActivity : Activity {
		TimePicker startTimePicker, endTimePicker;
		DatePicker datePicker;
		EditText titleEditText;
		Button saveButton;


		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.NewSession);

			//instantiate views
			startTimePicker = FindViewById<TimePicker>(Resource.Id.timePickerStart_NewSession);
			endTimePicker = FindViewById<TimePicker>(Resource.Id.timePickerEnd_NewSession);
			datePicker = FindViewById<DatePicker>(Resource.Id.datePicker_NewSession);
			titleEditText = FindViewById<EditText>(Resource.Id.editTextTitle_NewSession);
			saveButton = FindViewById<Button>(Resource.Id.buttonSave_NewSession);

			//populate fields with bundle data if possible
			if (savedInstanceState != null) {
				getBundleInfo(savedInstanceState);
			}

			saveButton.Click += delegate {
				/// send info back to SessionsActivity
				//create intent to go to SessionsActivity
				Intent intent = new Intent(this, typeof(SessionsActivity));
				//populate intent with data
				intent.PutExtras(getExtras());
				//successful intent transaction
				SetResult(Result.Ok, intent);
				//exit and send data
				Finish();
			};
		}

		protected override void OnSaveInstanceState(Bundle outState) {
			setBundleInfo(outState);

			
			base.OnSaveInstanceState(outState);
		}

		//populate views with bundle info
		private void getBundleInfo(Bundle inState) {
			//date, start time, end time, title
			startTimePicker.Hour = inState.GetInt("startHour");
			startTimePicker.Minute = inState.GetInt("startMinute");
			endTimePicker.Hour = inState.GetInt("endHour");
			endTimePicker.Minute = inState.GetInt("endMinute");
			titleEditText.Text = inState.GetString("title");
		}

		//populate bundle with view info
		private void setBundleInfo(Bundle outState) {
			int startMinute = startTimePicker.Minute;
			int startHour = startTimePicker.Hour;
			int endMinute = endTimePicker.Minute;
			int endHour = endTimePicker.Hour;
			string title = titleEditText.Text;

			outState.PutInt("startHour", startHour);
			outState.PutInt("startMinute", startMinute);
			outState.PutInt("endHour", endHour);
			outState.PutInt("endMinute", endMinute);
			outState.PutString("title", title);
		}

		//same as setBundleInfo, but returns a bundle
		private Bundle getExtras() {
			Bundle extras = new Bundle();

			int startMinute = startTimePicker.Minute;
			int startHour = startTimePicker.Hour;
			int endMinute = endTimePicker.Minute;
			int endHour = endTimePicker.Hour;
			string title = titleEditText.Text;

			extras.PutInt("startHour", startHour);
			extras.PutInt("startMinute", startMinute);
			extras.PutInt("endHour", endHour);
			extras.PutInt("endMinute", endMinute);
			extras.PutString("title", title);

			return extras;
		}
	}
}