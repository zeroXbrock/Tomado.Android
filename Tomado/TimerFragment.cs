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

	/// <summary>
	/// Fragment to display and contain timer views & code, respectively.
	/// </summary>
	public class TimerFragment : Android.Support.V4.App.Fragment {
		//view instances
		TextView timerTextView, typeTextView;
		Button workButton, pauseButton;

		//timer logic vars
		CountDownTimer countDownTimer;
		TimerType lastTimerType;
		int lastTimerTypeInt; //int representation b/c stupid xamarin; 0 is work, 1 is short, 2 is long, -1 is pause
		bool isTimerRunning = false;
		long interval, duration = 0;
		long remainingTimeInMillis = 0;
		long minuteInMillis = 60000;
		int shortBreaks = 0;
		bool isPaused = false;
		bool firstRun = true;
		
		Session fragmentSession; //var we'll use if we launch a timer from the sessions list

		public TimerFragment() {

		}

		public override void OnResume() {
			base.OnResume();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			//return base.OnCreateView(inflater, container, savedInstanceState);

			View rootView = inflater.Inflate(Resource.Layout.Timer, container, false);

			timerTextView = rootView.FindViewById<TextView>(Resource.Id.textViewTimer);
			typeTextView = rootView.FindViewById<TextView>(Resource.Id.textViewTimerType);
			workButton = rootView.FindViewById<Button>(Resource.Id.buttonWork);
			pauseButton = rootView.FindViewById<Button>(Resource.Id.buttonPause);

			if (fragmentSession == null)
				Init(savedInstanceState);
			else {
				//Init(sessionFromList);
				Bundle bundle = new Bundle();
				string title = fragmentSession.Title;
				bundle.PutString("title", title);

				Init(bundle);
			}

			#region button clicks

			workButton.Click += delegate {
				if (firstRun)
					firstRun = false;
				if (isPaused) {
					duration = remainingTimeInMillis;
					isPaused = false;
				}
				if (!isTimerRunning) {
					UpdateTimer();
					typeTextView.SetText(lastTimerType.ToString(), TextView.BufferType.Normal);
				}
				startTimer(duration);
			};
			pauseButton.Click += delegate {
				isPaused = true;
				countDownTimer.Cancel();
			};

			#endregion

			return rootView;
		}

		///helper functions to thin OnCreateView out
		
		//
		/// <summary>
		/// Sets local vars to bundle data.
		/// </summary>
		/// <param name="bundle"></param>
		private void SetTimerInfo(Bundle bundle) {
			remainingTimeInMillis = bundle.GetLong("remainingTimeInMillis");
			shortBreaks = bundle.GetInt("shortBreaks");
			isPaused = bundle.GetBoolean("isPaused");
			isTimerRunning = bundle.GetBoolean("isTimerRunning", isTimerRunning);
			lastTimerTypeInt = bundle.GetInt("lastTimerTypeInt");
			firstRun = bundle.GetBoolean("firstRun");
		}

		/// <summary>
		/// Converts int to TimerType enum.
		/// </summary>
		private void SetTimerTypeFromInt() {
			switch (lastTimerTypeInt) {
				case -1:
					lastTimerType = TimerType.Pause;
					break;
				case 0:
					lastTimerType = TimerType.Work;
					break;
				case 1:
					lastTimerType = TimerType.ShortBreak;
					break;
				case 2:
					lastTimerType = TimerType.LongBreak;
					break;
				default:
					lastTimerType = TimerType.Work;
					break;
			}
		}

		/// <summary>
		/// Initializes timer variables and sets textviews, gets state info from bundle when applicable.
		/// </summary>
		/// <param name="bundle">Bundle received from a fragment method override, typically OnCreate.</param>
		private void Init(Bundle bundle) {
			interval = 500; //interval set to 500 to prevent last-second "error" with CountDownTimer
			if (bundle == null) { // just started app
				//initialize timer vars
				duration = (long)CTimer.TimerLengths.Test;
				lastTimerType = TimerType.LongBreak;//set last type to long break so that we start on work //TODO: remove this line, probably

				typeTextView.SetText(TimerType.Work.ToString(), TextView.BufferType.Normal); //work is default
				timerTextView.SetText(getClockTimeLeft(duration), TextView.BufferType.Normal);
			}
			else {
				SetTimerInfo(bundle);

				SetTimerTypeFromInt();
				
				///starts timer on activity resume
				if (isPaused)
					timerTextView.SetText(getClockTimeLeft(remainingTimeInMillis), TextView.BufferType.Normal);
				
				if (remainingTimeInMillis > 0 && !isPaused) {
					startTimer(remainingTimeInMillis);
				}
				else if (!isTimerRunning || remainingTimeInMillis < interval)
					timerTextView.Text = Resource.String.Finished.ToString();

				if (fragmentSession == null)
					typeTextView.Text = lastTimerType.ToString();
				else
					typeTextView.Text = fragmentSession.Title;
			}

			if (firstRun) {
				typeTextView.SetText(TimerType.Work.ToString(), TextView.BufferType.Normal);
				timerTextView.SetText(getClockTimeLeft(CTimer.TimerLengths.Work), TextView.BufferType.Normal);
			}
		}


		/// <summary>
		/// Puts timer data in bundle.
		/// </summary>
		/// <param name="outState">Bundle to populate</param>
		private void SetBundleInfo(Bundle outState) {
			outState.PutLong("remainingTimeInMillis", remainingTimeInMillis);
			outState.PutInt("shortBreaks", shortBreaks);
			outState.PutBoolean("isPaused", isPaused);
			outState.PutBoolean("isTimerRunning", isTimerRunning);
			outState.PutBoolean("firstRun", firstRun);

			switch (lastTimerType) {
				case TimerType.LongBreak:
					lastTimerTypeInt = 2;
					break;
				case TimerType.ShortBreak:
					lastTimerTypeInt = 1;
					break;
				case TimerType.Work:
					lastTimerTypeInt = 0;
					break;
				case TimerType.Pause:
					lastTimerTypeInt = -1;
					break;
				default:
					lastTimerTypeInt = 0;
					break;
			}

			outState.PutInt("lastTimerTypeInt", lastTimerTypeInt);
		}

		public override void OnSaveInstanceState(Bundle outState) {
			SetBundleInfo(outState);

			base.OnSaveInstanceState(outState);
		}

		/// <summary>
		/// Initializes and starts the class timer.
		/// </summary>
		/// <param name="durationInMillis"></param>
		private void startTimer(long durationInMillis) {
			isTimerRunning = true;

			// make a new timer object
			countDownTimer = new CTimer(durationInMillis, interval, OnTick, OnFinish);
			countDownTimer.Start();
		}


		#region timer event handlers
		public void OnTick(long millisUntilFinished) {
			remainingTimeInMillis = millisUntilFinished;

			//update timer textview every whole second
			if (millisUntilFinished % 1000 > interval || millisUntilFinished == duration) {
				//set timer textview, format output time to seconds
				timerTextView.SetText(getClockTimeLeft(millisUntilFinished), TextView.BufferType.Normal);
			}
		}

		public void OnFinish() {
			remainingTimeInMillis = 0;

			timerTextView.SetText("Finished", TextView.BufferType.Normal);

			isTimerRunning = false;

		}

		public void OnNewTimer(Session session) {
			typeTextView.Text = session.Title;
			//SetFragmentSession(session);
		}
		#endregion

		#region helper functions to convert time
		
		/// <summary>
		/// Returns a number of minutes given a number of milliseconds.
		/// </summary>
		/// <param name="millisUntilFinished"></param>
		/// <returns></returns>
		private double getMinutesFromMillis(double millisUntilFinished) {
			double secsUntilFinished = getSecondsFromMillis(millisUntilFinished);
			double outputMins = (secsUntilFinished / 60) - (secsUntilFinished / 60) % 1;
			return outputMins;
		}
		/// <summary>
		/// Returns a number of seconds given a number of milliseconds.
		/// </summary>
		/// <param name="millisUntilFinished"></param>
		/// <returns></returns>
		private double getSecondsFromMillis(double millisUntilFinished) {

			return Math.Round(Math.Ceiling(millisUntilFinished / 1000) * 1000 * 0.001);
		}
		/// <summary>
		/// Returns a string containing how much time is left in m:ss format;
		/// </summary>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		private string getClockTimeLeft(double minutes, double seconds) {
			string outputSecs;

			//check for last minute
			if (minutes > 0)
				seconds = seconds % (minutes * 60);

			//check for last 10 seconds
			if (seconds < 10)
				outputSecs = "0" + seconds.ToString();
			else
				outputSecs = seconds.ToString();

			return minutes.ToString() + ":" + outputSecs;

		}
		/// <summary>
		/// Returns a string containing how much time is left in m:ss format;
		/// </summary>
		/// <param name="minutes"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		private string getClockTimeLeft(double millisUntilFinished) {
			double secsUntilFinished, minsUntilFinished;

			minsUntilFinished = getMinutesFromMillis(millisUntilFinished);
			secsUntilFinished = getSecondsFromMillis(millisUntilFinished);

			string outputTime = getClockTimeLeft(minsUntilFinished, secsUntilFinished);

			return outputTime;
		}
		#endregion

		/// <summary>
		/// Updates break info, session type, and duration; iterates lastTimerType through pomodoro cycle.
		/// </summary>
		private void UpdateTimer() {
			//if you just worked, start a break
			if (lastTimerType == TimerType.Work) {
				//set appropriate break time
				if (shortBreaks < 2) {
					shortBreaks++;
					//short break
					SetDuration((long)CTimer.TimerLengths.ShortBreak);
					SetTimerType(TimerType.ShortBreak);
				}
				else {
					//long break
					shortBreaks = 0;
					SetDuration((long)CTimer.TimerLengths.LongBreak);
					SetTimerType(TimerType.LongBreak);
				}
			}

			//if you just took a break, work
			else {
				//work
				SetDuration((long)CTimer.TimerLengths.Work);
				SetTimerType(TimerType.Work);
			}
		}

		/// <summary>
		/// Sets the class timer's type.
		/// </summary>
		/// <param name="type"></param>
		private void SetTimerType(TimerType type) {
			lastTimerType = type;
		}

		/// <summary>
		/// Sets the class timer duration.
		/// </summary>
		/// <param name="duration"></param>
		private void SetDuration(long duration) {
			this.duration = duration;
		}

		public void SetFragmentSession(Session session) {
			fragmentSession = session;
		}
	}
}