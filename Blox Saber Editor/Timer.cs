using System;
using System.Diagnostics;

namespace Blox_Saber_Editor
{
	class Timer
	{
		private readonly Stopwatch _sw = new Stopwatch();

		private double _last;

		private double _elapsed;

		public TimeSpan Elapsed
		{
			get => TimeSpan.FromMilliseconds(_elapsed);

			set
			{
				if (_sw.IsRunning)
					_sw.Restart();
				else
					_sw.Reset();

				_elapsed = value.TotalMilliseconds;
				_last = 0;
			}
		}

		public void Start() => _sw.Start();

		public void Stop() => _sw.Stop();

		public void Reset()
		{
			_sw.Reset();

			_elapsed = 0;
			_last = 0;
		}

		public void Update(double speed)
		{
			var elapsed = _sw.Elapsed.TotalMilliseconds;

			_elapsed += (elapsed - _last) * speed;

			_last = elapsed;
		}
	}
}