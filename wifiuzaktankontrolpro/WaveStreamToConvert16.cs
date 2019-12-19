using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wifiuzaktankontrolpro
{
    class WaveStreamToConvert16
    {
        //https://github.com/naudio/NAudio/issues/174
        public byte[] Convert16(byte[] input, int length, WaveFormat format, int rate)
        {
            if (length == 0)
                return new byte[0];
            using (var memStream = new MemoryStream(input, 0, length))
            {
                using (var inputStream = new RawSourceWaveStream(memStream, format))
                {
                    var sampleStream = new WaveToSampleProvider(inputStream);
                    var resamplingProvider = new WdlResamplingSampleProvider(sampleStream, rate);
                    var ieeeToPCM = new SampleToWaveProvider16(resamplingProvider);

                    /*var sampleStreams = new StereoToMonoProvider16(ieeeToPCM);
                    sampleStreams.RightVolume = 0.5f;
                    sampleStreams.LeftVolume = 0.5f;*/
                    return readStream(ieeeToPCM, length);
                }
            }
        }

        private byte[] readStream(IWaveProvider waveStream, int length)
        {
            byte[] buffer = new byte[length];
            using (var stream = new MemoryStream())
            {
                int read;
                while ((read = waveStream.Read(buffer, 0, length)) > 0)
                {
                    stream.Write(buffer, 0, read);
                }
                return stream.ToArray();
            }
        }

        /*Bu değiştirdiğim ses örnekleyicisinin birden waveformat değeri ile pcm çıkış yaratılabilir ama şu anlık bize gerekmiyor.
        public byte[] myConverter(byte[] input, int length, WaveFormat format)
        {
            if (length == 0)
                return new byte[0];
                using (var inputStream = new RawSourceWaveStream(input,0,length, format))
                {
                    var outFormat = new WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels);
                    var resampler = new MediaFoundationResampler(inputStream, outFormat);
                    //MessageBox.Show(resampler .WaveFormat.SampleRate.ToString()+ " - "+ resampler.WaveFormat.Encoding.ToString());
                    //var resamplingProvider = new WdlResamplingSampleProvider(sampleStream, 16000);
                    //var ieeeToPCM = new SampleToWaveProvider16(resampler.ToSampleProvider());
                    
                    //var sampleStreams = new StereoToMonoProvider16(ieeeToPCM);
                    //sampleStreams.RightVolume = 0.7f;
                    //sampleStreams.LeftVolume = 0.7f;
                    return readStream(resampler, length);
                }
        }*/

        /*
        void bilgisayarSesleriniOku()
        {
            //Bilgisayar Seslerini Okumak için            
            waveInStream = new WasapiLoopbackCapture(); // Bilgisayardaki tüm sesleri verir.                        
            waveInStream.DataAvailable += new EventHandler<WaveInEventArgs>(this.OnDataAvailable); //Ses bilgileri eventa gidecek.
            waveInStream.RecordingStopped += new EventHandler<StoppedEventArgs>(this.OnDataStopped); //Durdurulduğunda çalışacak event.


            // ###############  TEST  ###############            

            //WaveFormat wf = waveInStream.WaveFormat; //48000 Hz, 2 , 32 bit varsayılan neyse onu alır bilgisayardaki.ENCODING = ieeefloat / 32 bit length 38400

            //WaveFileWriter wFw;
            //aveWriter = new WaveFileWriter(streaming, waveIn.WaveFormat);
            //wFw = new WaveFileWriter("xxx.wav", csw);

            //waveInStream.StartRecording();

            //TEST İÇİN
            /*WaveInProvider wip = new WaveInProvider(waveInStream);
            DirectSoundOut dso = new DirectSoundOut(); //Direk hoparlör den dışarıya sesi aktarmak için test için kullanıldı.
            dso.Init(wip);
            waveInStream.StartRecording(); //Bilgisayardaki sesler okunuyor.
            dso.Play(); //TEST İÇİN
        }*/
    }
}
