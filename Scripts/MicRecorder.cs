using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NekomimiDaimao
{
    ///  https://gist.github.com/nekomimi-daimao/a14301d7008d0a1c7e55977d6d9e2cc1
    public class MicRecorder : MonoBehaviour
    {
        public MicIn micIn;
        public Config config;
        private const int HeaderLength = 44;
        private const int RescaleFactor = 32767;

        private FileStream _fileStream;
        private UniTask _recordingCoroutine;

        public bool IsRecording { get; private set; } = false;
        public bool task_processing = false;


        public void StartRecord(string target_path = null)
        {
            task_processing = true;
            var path = target_path ?? $"{Application.streamingAssetsPath}/test.wav";
            if (IsRecording || !_recordingCoroutine.Status.IsCompleted()) return;
            IsRecording = true;
            _recordingCoroutine = UniTask.Create(async () => {await StartRecordCoroutine(path);});
        }

        /// <summary>
        /// yield return StartCoroutine(MicRecorder.StopRecord());
        /// </summary>
        public async UniTask StopRecord()
        {
            IsRecording = false;
            while (task_processing) await UniTask.Yield(); //音声認識タスクが全て終了するまで待機
            
        }


        private async UniTask StartRecordCoroutine(string path)
        {
            try {
                _fileStream = new FileStream(path, FileMode.Create);
                const byte emptyByte = new byte();
                for (var count = 0; count < HeaderLength; count++) _fileStream.WriteByte(emptyByte);
                if (Microphone.devices.Length == 0) return;

                var buffer = new float[config.MaxSec * micIn.SAMPLE_RATE];
                //var head = 0;
                int pos;
                while (IsRecording) {
                    pos = Microphone.GetPosition(config.MicDevice);
                    if (pos >= 0 && pos != config.MicHead)
                    {
                        config.microphoneSource.clip.GetData(buffer, 0);
                        var writeBuffer = CreateWriteBuffer(pos, config.MicHead, buffer);
                        //Debug.Log(String.Join(",", writeBuffer));
                        ConvertAndWrite(writeBuffer);
                        config.MicHead = pos;
                    }
                    await UniTask.Yield();
                }

                pos = Microphone.GetPosition(config.MicDevice);
                if (pos >= 0 && pos != config.MicHead) {
                    config.microphoneSource.clip.GetData(buffer, 0);
                    var writeBuffer = CreateWriteBuffer(pos, config.MicHead, buffer);
                    ConvertAndWrite(writeBuffer);
                }
                WriteWavHeader(_fileStream, config.microphoneSource.clip.channels, micIn.SAMPLE_RATE);
            }
            catch (Exception ex) {
                //#if UNITY_EDITOR
                Debug.Log("Exception: " + ex.Message);
                //#endif
            }
            finally {
                _fileStream?.Dispose();
                _fileStream = null;
                task_processing = false;
            }
        }

        private static float[] CreateWriteBuffer(int pos, int head, float[] buffer)
        {
            float[] writeBuffer;
            if (head < pos)
            {
                writeBuffer = new float[pos - head];
                Array.Copy(buffer, head, writeBuffer, 0, writeBuffer.Length);
            }
            else
            {
                writeBuffer = new float[(buffer.Length - head) + pos];
                Array.Copy(buffer, head, writeBuffer, 0, (buffer.Length - head));
                Array.Copy(buffer, 0, writeBuffer, (buffer.Length - head), pos);
            }

            return writeBuffer;
        }

        private void ConvertAndWrite(float[] dataSource)
        {
            Int16[] intData = new Int16[dataSource.Length];
            var bytesData = new byte[dataSource.Length * 2];
            for (int i = 0; i < dataSource.Length; i++) {
                intData[i] = (short) (dataSource[i] * RescaleFactor);
                var byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            _fileStream.Write(bytesData, 0, bytesData.Length);
        }

        private void WriteWavHeader(FileStream fileStream, int channels, int samplingFrequency)
        {
            var samples = ((int) fileStream.Length - HeaderLength) / 2;

            fileStream.Flush();
            fileStream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            //UInt16 _two = 2;
            UInt16 _one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(_one);
            fileStream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(channels);
            fileStream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(samplingFrequency);
            fileStream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(samplingFrequency * channels * 2);
            fileStream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort) (channels * 2);
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            fileStream.Write(subChunk2, 0, 4);

            fileStream.Flush();
            fileStream.Close();
        }
    }
}
