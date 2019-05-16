using System;
using Blox_Saber_Editor.SoundTouch;
using NAudio.Wave;

namespace Blox_Saber_Editor
{
	class MusicPlayer : IDisposable
	{
		private WaveStream _music;
		private WaveChannel32 _volumeStream;
		private WaveOutEvent _player;
		private VarispeedSampleProvider _speedControl;
		
		private readonly BetterTimer _time;

		private object locker = new object();

		public MusicPlayer()
		{
			_player = new WaveOutEvent();
			_time = new BetterTimer();
		}

		public void Load(string file)
		{
			_music?.Dispose();
			_volumeStream?.Dispose();
			_player?.Dispose();
			_speedControl?.Dispose();

			var reader = new AudioFileReader(file);
			_music = reader;
			_volumeStream = new WaveChannel32(_music, Volume, 0);
			_player = new WaveOutEvent();

			_speedControl = new VarispeedSampleProvider(reader, 150, new SoundTouchProfile(true, true));

			Init();

			Reset();
		}

		public void Init() => _player.Init(_speedControl);
		public void Play()
		{
			lock (locker)
			{
				var time = CurrentTime;

				if (Progress >= 0.999998)
				{
					time = TimeSpan.Zero;
				}

				Stop();

				CurrentTime = time;

				_player.Play();
				_time.Start();
			}
		}
		public void Pause()
		{
			lock (locker)
			{
				_time.Stop();
				_player.Pause();
			}
		}
		public void Stop()
		{
			lock (locker)
			{
				_time.Reset();
				_player.Stop();
			}
		}

		public float Speed
		{
			get => _speedControl?.PlaybackRate ?? 1;

			set
			{
				lock (locker)
				{
					var wasPlaying = IsPlaying;

					Pause();
					_time.SetSpeed(value);
					var time = _time.Elapsed;
					Stop();

					_speedControl.PlaybackRate = value;

					CurrentTime = time;

					Init();

					if (wasPlaying)
						Play();
				}
			}
		}

		public float Volume
		{
			get => _player.Volume;

			set => _player.Volume = value;
		}

		public void Reset()
		{
			Stop();

			_music.CurrentTime = TimeSpan.Zero;
		}

		public bool IsPlaying => _player.PlaybackState == PlaybackState.Playing;
		public bool IsPaused => _player.PlaybackState == PlaybackState.Paused;

		public TimeSpan TotalTime => _music?.TotalTime ?? TimeSpan.Zero;

		public TimeSpan CurrentTime
		{
			get
			{
				lock (locker)
				{
					if (_music == null)
						return TimeSpan.Zero;

					var time = _time.Elapsed;

					time = time > _music.TotalTime ? TotalTime : time;

					return time;
				}
			}
			set
			{
				lock (locker)
				{
					if (_music == null)
						return;

					Stop();

					_music.CurrentTime = value;
					_time.Elapsed = value;

					_speedControl.Reposition();

					Pause();
				}
			}
		}

		public double Progress => TotalTime == TimeSpan.Zero ? 0 : Math.Min(1, CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

		public void Dispose()
		{
			_player?.Dispose();
			_speedControl?.Dispose();
			_music?.Dispose();
			_volumeStream?.Dispose();
		}
	}
}