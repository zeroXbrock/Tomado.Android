using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Tomado {
	/// <summary>
	/// Fragment that contains dialogs for user to create a new Session.
	/// </summary>
	public class NewSessionFragment :  Android.Support.V4.App.DialogFragment, DatePickerDialog.IOnDateSetListener, TimePickerDialog.IOnTimeSetListener {
		private GetNewSessionListener onGetNewSessionListener;
		
		//view instances
		Button saveButton, cancelButton;
		EditText timeEditText, dateEditText, titleEditText;
		Switch recurringSwitch;
		LinearLayout recurringLayout;

		//session vars
		int _year, _month, _day, _hour, _minute;
		DateTime sessionDateTime = DateTime.Now;
		string _title;
		bool recurring;
		
		public interface GetNewSessionListener{
			void OnAddNewSession(DateTime dateTime, string title, bool recurring);
		}

		public override void OnActivityCreated(Bundle savedInstanceState) {
			base.OnActivityCreated(savedInstanceState);

			//open keyboard automatically when dialog opens
			Dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
		}

		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			//instantiate listener
			try {
				onGetNewSessionListener = (GetNewSessionListener)TargetFragment;
			}
			catch (Exception e) {
				Log.Debug("OnGetNewSessionListener cast exception", e.Message);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			Dialog.SetTitle("New session");
			Dialog.Window.SetBackgroundDrawableResource(Resource.Color.base_app_complementary_color);
			
			View view = inflater.Inflate(Resource.Layout.NewSessionDialog, container, false);

			saveButton = view.FindViewById<Button>(Resource.Id.buttonSave_NewSession);
			cancelButton = view.FindViewById<Button>(Resource.Id.buttonCancel_NewSession);
			timeEditText = view.FindViewById<EditText>(Resource.Id.editTextTime_NewSession);
			dateEditText = view.FindViewById<EditText>(Resource.Id.editTextDate_NewSession);
			titleEditText = view.FindViewById<EditText>(Resource.Id.editTextTitle_NewSession);
			recurringSwitch = view.FindViewById<Switch>(Resource.Id.switchRecurring_NewSession);
			recurringLayout = view.FindViewById<LinearLayout>(Resource.Id.Layout_Recurring_NewSession);
			
			//TODO: This weekday button code should really be encapsulated into another fragment since it's being used identically elsewhere (SessionAdapter)
			List<WeekdayButton> weekdayButtons = new List<WeekdayButton>();
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonSunday_Recurring), DayOfWeek.Sunday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonMonday_Recurring), DayOfWeek.Monday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonTuesday_Recurring), DayOfWeek.Tuesday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonWednesday_Recurring), DayOfWeek.Wednesday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonThursday_Recurring), DayOfWeek.Thursday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonFriday_Recurring), DayOfWeek.Friday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonSaturday_Recurring), DayOfWeek.Saturday));
						
			//set default values
			SetDefaultTimeValues();

			//update text views
			UpdateDateTimeInfo();

			saveButton.Click += delegate {
				///send session data back; launch event, close fragment
				//get info to save
				sessionDateTime = new DateTime(_year, _month, _day, _hour, _minute, 0);
				_title = titleEditText.Text;
				recurring = recurringSwitch.Checked;

				//send data out
				onGetNewSessionListener.OnAddNewSession(sessionDateTime, _title, recurring);

				//close fragment
				Dismiss();
			};
			cancelButton.Click += delegate {
				Dismiss();
			};

			timeEditText.Focusable = false;
			dateEditText.Focusable = false;

			timeEditText.Click += delegate {
				var dialog = new TimePickerDialogFragment(Activity, DateTime.Now, this);
				dialog.Show(FragmentManager, null);
			};
			dateEditText.Click += delegate {
				var dialog = new DatePickerDialogFragment(Activity, DateTime.Now.AddMonths(-1), this);
				dialog.Show(FragmentManager, null);
			};

			recurringSwitch.Click += delegate {
				recurringLayout.Visibility = recurringSwitch.Checked ? ViewStates.Visible : ViewStates.Gone;
				recurring = recurringSwitch.Checked;
			};

			//set weekday button clicks
			foreach (var b in weekdayButtons) {
				b.Button.Click += delegate {
					Log.Debug("weekday", b.Button.Text);
					b.Toggled = !b.Toggled;
					Log.Debug("weekday toggle", b.Toggled.ToString());
				};
			}

			return view;
		}

		///event handlers for date/time picker dialogs
		public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth) {
			//set time vars
			_year = year;
			_month = monthOfYear;
			_day = dayOfMonth;

			UpdateDateTimeInfo();
		}
		public void OnTimeSet(TimePicker view, int hourOfDay, int minute) {
			//set time vars
			_hour = hourOfDay;
			_minute = minute;

			UpdateDateTimeInfo();
		}

		//populate the date/time vars w/ default values
		private void SetDefaultTimeValues() {
			_year = sessionDateTime.Year;
			_month = sessionDateTime.Month-1;
			_day = sessionDateTime.Day;
			_hour = sessionDateTime.Hour;
			_minute = sessionDateTime.Minute;
		}

		//updates DateTime class var and the edittext views
		private void UpdateDateTimeInfo() {
			sessionDateTime = new DateTime(_year, _month+1, _day, _hour, _minute, 0);

			timeEditText.Text = sessionDateTime.ToShortTimeString();
			dateEditText.Text = sessionDateTime.ToShortDateString();
		}
	}
}