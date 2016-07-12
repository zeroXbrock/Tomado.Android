using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Tomado {
	[Activity(Label = "Tomado", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity {
		
		//timer logic vars
		CountDownTimer countDownTimer;
		TimerType lastTimerType;
		bool isTimerRunning = false;
		long interval, duration = 0;
		long remainingTimeInMillis = 0;
		long minuteInMillis = 60000;
		int shortBreaks = 0;
		bool isPaused = false;

		//view instances
		TextView timerTextView, typeTextView;
		Button pauseButton, workButton;

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			interval = 500; //interval set to 500 to prevent last-second "error" with CountDownTimer

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Timer);

			//get references to our layout items
			timerTextView = FindViewById<TextView>(Resource.Id.textViewTimer);
			typeTextView = FindViewById<TextView>(Resource.Id.textViewTimerType);
			pauseButton = FindViewById<Button>(Resource.Id.buttonPause);
			workButton = FindViewById<Button>(Resource.Id.buttonWork);

			//initialize timer vars
			lastTimerType = TimerType.LongBreak;//start at work
			updateTimer();

			//initialize text views
			typeTextView.SetText(TimerType.Work.ToString(), TextView.BufferType.Normal);
			OnTick(duration);//much easier to just tick once to set up the timer text view

			#region button clicks

			workButton.Click += delegate {
				if (isPaused) {
					duration = remainingTimeInMillis;
					isPaused = false;
				}

				typeTextView.SetText(lastTimerType.ToString(), TextView.BufferType.Normal);
				startTimer(duration);
			};
			pauseButton.Click += delegate {
				isTimerRunning = false;
				isPaused = true;
				countDownTimer.Cancel();
			};

			#endregion
		}

		//helper function to start timer
		private void startTimer(long durationInMillis) {
			if (!isTimerRunning) {
				isTimerRunning = true;

				// make a new timer object
				countDownTimer = new CTimer(durationInMillis, interval, OnTick, OnFinish);
				countDownTimer.Start();
			}
		}


		#region timer event functions
		//(delegated) event methods for timer to update UI
		public void OnTick(long millisUntilFinished) {
			remainingTimeInMillis = millisUntilFinished;

			//update time every whole second
			if (millisUntilFinished % 1000 > interval || millisUntilFinished > (duration - interval)) {
				//format output time to seconds
				double secsUntilFinished = Math.Round(millisUntilFinished * 0.001);

				//create vars to display minutes and seconds
				double outputMins = (secsUntilFinished / 60) - (secsUntilFinished / 60) % 1;
				double outputSecs;
				string outputSecsString;

				//check for last minute
				if (outputMins > 0)
					outputSecs = secsUntilFinished % (outputMins * 60);
				else
					outputSecs = secsUntilFinished;

				//check for last 10 seconds
				if (outputSecs < 10)
					outputSecsString = "0" + outputSecs.ToString();
				else
					outputSecsString = outputSecs.ToString();

				string outputTime = outputMins.ToString() + ":" + outputSecsString;

				timerTextView.SetText(outputTime, TextView.BufferType.Normal);
			}
		}

		public void OnFinish() {
			remainingTimeInMillis = 0;
			
			updateTimer();


			timerTextView.SetText("Finished", TextView.BufferType.Normal);
			isTimerRunning = false;

		}
		#endregion

		//updates break info, session type, and duration
		private void updateTimer() {
			//if you just worked, start a break
			if (lastTimerType == TimerType.Work) {
				//set appropriate break time
				if (shortBreaks < 2) {
					shortBreaks++;
					//short break
					//duration = minuteInMillis * 5; //5 mins
					duration = 2000;
					lastTimerType = TimerType.ShortBreak;
				}
				else {
					//long break
					shortBreaks = 0;
					//duration = minuteInMillis * 10;
					duration = 3000;
					lastTimerType = TimerType.LongBreak;
				}
			}

			//if you just took a break, work
			else {
				//work
				//duration = minuteInMillis * 25;
				duration = 4000;
				lastTimerType = TimerType.Work;
			}
		}
	}
}

