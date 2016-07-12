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
	//instantiate an instance of this class to get a CTimer object that calls your events on tick and finish
	public class CTimer : CountDownTimer {

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
			}
		}
		public override void OnFinish() {
			Finish();
		}
	}

	public enum TimerType {
		Work, ShortBreak, LongBreak, Pause
	}
}