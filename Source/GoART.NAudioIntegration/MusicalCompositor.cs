using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Dsp;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GoART.NAudioIntegration
{
    public class MusicalCompositor
    {
        private readonly float[] samples;
        private readonly int sampleRate;
        private readonly Random r;

        public MusicalCompositor(TimeSpan timeSpan, int sampleRate = 48000)
        {
            this.sampleRate = sampleRate;
            this.samples = new float[ConvertToSample(timeSpan)];
            this.r = new Random();
        }

        /// <summary>
        /// number of Sample in timespan
        /// </summary>
        /// <param name="length">Second</param>
        private int ConvertToSample(TimeSpan length) => (int)Math.Round(sampleRate * length.TotalSeconds);
        
        private float ConvertToDbFactor(float volume) => (float)Math.Pow(10, volume / 20);
        
        /// <summary>
        /// karplus-strong-algorithm
        /// </summary>
        /// <param name="frequency">Hz</param>
        /// <param name="length">Second</param>
        /// <param name="volume">Db Factor</param>
        public MusicalCompositor Tune(float frequency, TimeSpan offset, TimeSpan length, float volume=0)
        {
            Kps(frequency, ConvertToSample(offset), ConvertToSample(length), ConvertToDbFactor(volume));
            return this;
        }

        /// <summary>
        /// karplus-strong-algorithm
        /// </summary>
        /// <param name="frequency">Hz</param>
        /// <param name="offset">start sample</param>
        /// <param name="length">length of samples</param>
        /// <param name="volume">Db Factor</param>
        private void Kps(float frequency, int offset, int length, float dbFactor = 1)
        {
            // comment from https://blog.demofox.org/2016/06/16/synthesizing-a-pluked-string-sound-with-the-karplus-strong-algorithm/

            var kpsBufferSize = Math.Round(sampleRate / frequency); //  "What guitar is ever perfectly in tune, right?!"
            
            ///What we want to do is create two Queues, Q1 and Q2.
            var Q1 = new Queue<float>();
            var Q2 = new Queue<float>();
            
            var r = new Random();

            for (int i = 0; i < kpsBufferSize; i++)
            {
                Q1.Enqueue((float)(r.NextDouble()*2 - 1f)); /// Into Q1 we will enqueue n random numbers between -1.0 and 1.0.
                Q2.Enqueue(0f); /// In Q2 we will enqueue a single 0.0.
            }

            var low = BiQuadFilter.LowPassFilter(sampleRate, frequency * 1.5f, 1000f); //NAudio Filter
            var high = BiQuadFilter.HighPassFilter(sampleRate, frequency * 0.5f, 1000f); //NAudio Filter
            
            for (int i = 0; i < length; i++)
            {
                var a = Q1.Dequeue();
                var b = Q2.Dequeue();
                var c = 0.99f * ((a + b) / 2f);
                Q1.Enqueue(c);
                Q2.Enqueue(a);
                samples[offset+i] += high.Transform(low.Transform(c))* dbFactor;
            }
        }
        
        public void ToWav(string filename)
        {
            var format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            using (WaveFileWriter w = new WaveFileWriter(filename, format))
            {
                w.WriteSamples(samples, 0, samples.Length);
            }
        }

        public void ToWav(Stream s)
        {
            var format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            using (WaveFileWriter w = new WaveFileWriter(s, format))
            {
                w.WriteSamples(samples, 0, samples.Length);
            }
        }
    }
}