using System;
using System.Diagnostics;

namespace Blox_Saber_Editor
{
	class Timer
	{
		private long _currentTime;
		private long _lastTime;

		private bool _isRunning;

		private long _elapsed;

		public TimeSpan Elapsed
		{
			get
			{
				Update(EditorWindow.Instance.MusicPlayer.Speed);

				return TimeSpan.FromTicks(_elapsed);
			}

			set
			{
				_elapsed = value.Ticks;

				Update(EditorWindow.Instance.MusicPlayer.Speed);
			}
		}

		public void Start()
		{
			_currentTime = DateTime.Now.Ticks;
			_lastTime = _currentTime;

			_isRunning = true;
		}

		public void Stop()
		{
			_currentTime = DateTime.Now.Ticks;
			if (_isRunning)
				_elapsed += (long)Math.Round((_currentTime - _lastTime) * EditorWindow.Instance.MusicPlayer.Speed);
			_lastTime = _currentTime;

			_isRunning = false;
		}

		public void Reset()
		{
			_isRunning = false;

			_currentTime = DateTime.Now.Ticks;
			_lastTime = _currentTime;

			_elapsed = 0;
		}

		public void Update(double speed)
		{
			_currentTime = DateTime.Now.Ticks;
			if (_isRunning)
				_elapsed += (long) Math.Round((_currentTime - _lastTime) * speed);
			_lastTime = _currentTime;
		}
	}
}