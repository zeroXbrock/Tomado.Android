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
	/// Class to get a modified CountDownTimer object that calls your events on tick and finish.
	/// </summary>
	public class CTimer : CountDownTimer {
		long millisUntilFinished;

		public long MillisUntilFinished {
			get { return millisUntilFinished; }
		}

		//delegates to communicate with UI from timer code
		public delegate void TickEvent(long millisUntilFinished);
		public delegate void FinishEvent();

		//events to be called
		public event TickEvent Tick;
		public event FinishEvent Finish;

		//constructor
		public CTimer(long duration, long interval, TickEvent onTick, FinishEvent onFinish) : base(duration, interval) {
			Tick = onTick;
			Finish = onFinish;
		}

		/// Generic methods that are populated by calling class's arguments to the constructor
		public override void OnTick(long millisUntilFinished) {
			if (Tick != null) {
				Tick(millisUntilFinished);
				this.millisUntilFinished = millisUntilFinished;
			}
		}
		public override void OnFinish() {
			Finish();
		}

		public static class TimerLengths {
			private const int minuteInMillis = 1000 * 60;
			public const double 
				/*
				Work =		25 * minuteInMillis, 
				ShortBreak = 5 * minuteInMillis, 
				LongBreak = 10 * minuteInMillis;
				 */
				Work = 4000,
				ShortBreak = 2000,
				LongBreak = 3000;
		}
	}

	public enum TimerType {
		Work, ShortBreak, LongBreak, Pause
	}

	
}