using System;
using System.Collections.Generic;
using NAudio.Wave;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Blox_Saber_Editor
{
	class SoundPlayer : IDisposable
	{
		private AudioContext _context;

		private readonly Dictionary<string, Tuple<int, int>> _sounds = new Dictionary<string, Tuple<int, int>>();

		private string _lastId;

		public void Init()
		{
			_context = new AudioContext();

			AL.Listener(ALListenerf.Gain, 1);
			AL.Listener(ALListener3f.Position, 0, 0, 0);
			AL.Listener(ALListener3f.Velocity, 0, 0, 0);
		}

		public void Cache(string id, string file)
		{
			//create a buffer
			byte[] data;
			WaveFormat format;

			using (var afr = new AudioFileReader(file))
			{
				data = new byte[afr.Length];

				var provider = afr.ToSampleProvider().ToStereo().ToWaveProvider16();
				provider.Read(data, 0, data.Length);

				format = provider.WaveFormat;
			}

			var buffer = AL.GenBuffer();
			AL.BufferData(buffer, ALFormat.Stereo16, data, data.Length, format.SampleRate);

			//create audio source
			var source = AL.GenSource();
			AL.Source(source, ALSourcef.Gain, 0f);
			AL.Source(source, ALSourcef.Pitch, 1);
			AL.Source(source, ALSource3f.Position, 0, 0, 0);

			AL.BindBufferToSource(source, buffer);

			_sounds.Add(id, new Tuple<int, int>(source, buffer));
		}

		public void Play(string id, float volume = 1)
		{
			if (_sounds.TryGetValue(id, out var sound))
			{
				if (id != _lastId)
				{
					_lastId = id;

					AL.Source(sound.Item1, ALSourcei.Buffer, sound.Item2);
				}

				AL.Source(sound.Item1, ALSourcef.Gain, volume);
				AL.SourcePlay(sound.Item1);
			}
		}

		public void Dispose()
		{
			foreach (var tuple in _sounds.Values)
			{
				AL.DeleteSource(tuple.Item1);
				AL.DeleteBuffer(tuple.Item2);
			}

			_context.Dispose();
		}
	}
}