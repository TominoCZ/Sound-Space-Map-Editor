using System;
using System.Runtime.InteropServices;

namespace Blox_Saber_Editor
{
	class BetterTimer
	{
		private const string _lib = "TimerLib.dll";

		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr Create(); 
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Delete(IntPtr obj);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Start_(IntPtr obj);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Stop_(IntPtr obj);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Reset_(IntPtr obj);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetSpeed_(IntPtr obj, double speed);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetTime_(IntPtr obj, double time);
		[DllImport(_lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern double GetElapsedSeconds_(IntPtr obj);

		private readonly IntPtr _obj;

		public TimeSpan Elapsed
		{
			get => TimeSpan.FromSeconds(GetElapsedSeconds_(_obj));
			set => SetTime_(_obj, value.TotalSeconds);
		}

		public BetterTimer()
		{
			_obj = Create();
		}

		public void Start() => Start_(_obj);
		public void Stop() => Stop_(_obj);
		public void Reset() => Reset_(_obj);
		public void SetSpeed(double speed) => SetSpeed_(_obj, speed);

		~BetterTimer() => Delete(_obj);
	}
}