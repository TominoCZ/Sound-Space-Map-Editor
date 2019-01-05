using System;
using NAudio.Wave;
using VarispeedDemo.SoundTouch;

namespace Blox_Saber_Editor
{
	class MusicPlayer : IDisposable
	{
		private WaveStream _music;
		private WaveChannel32 _volumeStream;
		private WaveOutEvent _player;
		private VarispeedSampleProvider _speedControl;

		private readonly Timer _time = new Timer();

		public MusicPlayer()
		{
			_player = new WaveOutEvent();
		}

		public void Load(string file)
		{
			_music?.Dispose();
			_volumeStream?.Dispose();
			_player?.Dispose();
			_speedControl?.Dispose();

			var reader = new AudioFileReader(file);
			_music = reader;
			_volumeStream = new WaveChannel32(_music);
			_player = new WaveOutEvent();

			_speedControl = new VarispeedSampleProvider(reader, 1000, new SoundTouchProfile(true, true));

			Init();

			Reset();
		}

		public void Init() => _player.Init(_speedControl);
		public void Play()
		{
			if (TotalTime == CurrentTime)
			{
				CurrentTime = TimeSpan.Zero;
			}
			
			_time.Start();
			_player.Play();
		}
		public void Pause()
		{
			_time.Stop();
			_player.Pause();
		}
		public void Stop()
		{
			_time.Reset();
			_player.Stop();
		}

		public float Speed
		{
			get => _speedControl?.PlaybackRate ?? 1;

			set
			{
				var wasPlaying = IsPlaying;

				Pause();
				var time = _time.Elapsed;
				Stop();

				_speedControl.PlaybackRate = value;

				CurrentTime = time;

				Init();

				if (wasPlaying)
					Play();
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

		public void Update()
		{
			_time.Update(Speed);
		}

		public bool IsPlaying => _player.PlaybackState == PlaybackState.Playing;
		public bool IsPaused => _player.PlaybackState == PlaybackState.Paused;

		public TimeSpan TotalTime => _music?.TotalTime ?? TimeSpan.Zero;

		public TimeSpan CurrentTime
		{
			get
			{
				if (_music == null)
					return TimeSpan.Zero;

				Update();

				var time = _time.Elapsed;

				time = time > _music.TotalTime ? _music.TotalTime : time;

				return time;
			}
			set
			{
				if (_music == null)
					return;

				_music.CurrentTime = value;
				_time.Elapsed = value;

				_speedControl.Reposition();
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