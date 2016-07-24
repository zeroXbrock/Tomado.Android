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
using Android.Support.V4.View;
using Android.Support.V4.App;

namespace Tomado {
	public class FragmentAdapter : FragmentPagerAdapter {
		//list to hold fragments
		private List<Android.Support.V4.App.Fragment> _fragmentList = new List<Android.Support.V4.App.Fragment>();

		//blank constructor
		public FragmentAdapter(Android.Support.V4.App.FragmentManager fm) : base(fm) { }

		//overrides
		public override int Count {
			get { return _fragmentList.Count; }
		}
		public override Android.Support.V4.App.Fragment GetItem(int position) {
			return _fragmentList[position];
		}

		public void AddFragment(Android.Support.V4.App.Fragment fragment) {
			_fragmentList.Add(fragment);
		}
	}
}