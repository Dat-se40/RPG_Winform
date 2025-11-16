using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BTLT04.Components
{
    /// <summary>
    /// Quản lý âm thanh cho ứng dụng WinForms - Singleton Pattern
    /// Hỗ trợ MP3, WAV, OGG và nhiều format khác
    /// </summary>
    public sealed class SoundManager
    {
        private static readonly Lazy<SoundManager> _instance =
            new Lazy<SoundManager>(() => new SoundManager());

        private IWavePlayer _backgroundPlayer;
        private WaveStream _backgroundReader;
        private Dictionary<string, CachedSound> _effectSounds;
        private bool _isBackgroundPlaying;
        private float _backgroundVolume = 0.5f;
        private float _effectVolume = 0.8f;

        /// <summary>
        /// Instance duy nhất của SoundManager
        /// </summary>
        public static SoundManager Instance => _instance.Value;

        private SoundManager()
        {
            try
            {
                _effectSounds = new Dictionary<string, CachedSound>();
                _isBackgroundPlaying = false;
            }
            catch (Exception ex)
            {
                LogError("Khởi tạo SoundManager", ex);
            }
        }

        #region Background Music

        /// <summary>
        /// Phát nhạc nền (hỗ trợ MP3, WAV, OGG, FLAC, AIFF)
        /// </summary>
        public void PlayBackground(string filePath, bool loop = true)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath), "Đường dẫn file không được để trống");
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file âm thanh: {filePath}");
                }

                StopBackground();

                // Tạo reader phù hợp với định dạng file
                _backgroundReader = CreateAudioReader(filePath);

                // Chỉ set volume nếu là AudioFileReader
                if (_backgroundReader is AudioFileReader audioReader)
                {
                    audioReader.Volume = _backgroundVolume;
                }

                _backgroundPlayer = new WaveOutEvent();
                _backgroundPlayer.Init(_backgroundReader);
                _backgroundPlayer.PlaybackStopped += (s, e) =>
                {
                    if (loop && _isBackgroundPlaying)
                    {
                        try
                        {
                            _backgroundReader.Position = 0;
                            _backgroundPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            LogError("Loop nhạc nền", ex);
                            _isBackgroundPlaying = false;
                        }
                    }
                    else
                    {
                        _isBackgroundPlaying = false;
                    }
                };

                _backgroundPlayer.Play();
                _isBackgroundPlaying = true;
            }
            catch (FileNotFoundException ex)
            {
                LogError("Phát nhạc nền", ex);
                MessageBox.Show($"Không tìm thấy file nhạc nền: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LogError("Phát nhạc nền", ex);
                MessageBox.Show($"Lỗi khi phát nhạc nền: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Dừng nhạc nền
        /// </summary>
        public void StopBackground()
        {
            try
            {
                _isBackgroundPlaying = false;

                if (_backgroundPlayer != null)
                {
                    _backgroundPlayer.Stop();
                    _backgroundPlayer.Dispose();
                    _backgroundPlayer = null;
                }

                if (_backgroundReader != null)
                {
                    _backgroundReader.Dispose();
                    _backgroundReader = null;
                }
            }
            catch (Exception ex)
            {
                LogError("Dừng nhạc nền", ex);
            }
        }

        /// <summary>
        /// Tạm dừng nhạc nền
        /// </summary>
        public void PauseBackground()
        {
            try
            {
                if (_backgroundPlayer != null && _isBackgroundPlaying)
                {
                    _backgroundPlayer.Pause();
                }
            }
            catch (Exception ex)
            {
                LogError("Tạm dừng nhạc nền", ex);
            }
        }

        /// <summary>
        /// Tiếp tục phát nhạc nền
        /// </summary>
        public void ResumeBackground()
        {
            try
            {
                if (_backgroundPlayer != null && _isBackgroundPlaying)
                {
                    _backgroundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                LogError("Tiếp tục nhạc nền", ex);
            }
        }

        /// <summary>
        /// Kiểm tra nhạc nền có đang phát không
        /// </summary>
        public bool IsBackgroundPlaying => _isBackgroundPlaying;

        /// <summary>
        /// Âm lượng nhạc nền (0.0 - 1.0)
        /// </summary>
        public float BackgroundVolume
        {
            get => _backgroundVolume;
            set
            {
                _backgroundVolume = Math.Max(0f, Math.Min(1f, value));
                if (_backgroundReader is AudioFileReader audioReader)
                {
                    audioReader.Volume = _backgroundVolume;
                }
            }
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Tải trước sound effect vào bộ nhớ
        /// </summary>
        public void LoadEffect(string effectName, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(effectName))
                {
                    throw new ArgumentNullException(nameof(effectName), "Tên effect không được để trống");
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath), "Đường dẫn file không được để trống");
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file âm thanh: {filePath}");
                }

                if (_effectSounds.ContainsKey(effectName))
                {
                    _effectSounds[effectName].Dispose();
                    _effectSounds.Remove(effectName);
                }

                var cachedSound = new CachedSound(filePath);
                _effectSounds[effectName] = cachedSound;
            }
            catch (FileNotFoundException ex)
            {
                LogError($"Tải effect '{effectName}'", ex);
                MessageBox.Show($"Không tìm thấy file effect: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LogError($"Tải effect '{effectName}'", ex);
                MessageBox.Show($"Lỗi khi tải effect: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Phát sound effect đã được load trước
        /// </summary>
        public void PlayEffect(string effectName)
        {
            try
            {
                if (string.IsNullOrEmpty(effectName))
                {
                    throw new ArgumentNullException(nameof(effectName));
                }

                if (_effectSounds.ContainsKey(effectName))
                {
                    PlayCachedSound(_effectSounds[effectName]);
                }
                else
                {
                    throw new KeyNotFoundException($"Effect '{effectName}' chưa được load");
                }
            }
            catch (KeyNotFoundException ex)
            {
                LogError($"Phát effect '{effectName}'", ex);
                MessageBox.Show($"Effect chưa được tải: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LogError($"Phát effect '{effectName}'", ex);
            }
        }

        /// <summary>
        /// Phát sound effect trực tiếp từ file (không cần load trước)
        /// </summary>
        public void PlayEffectDirect(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file âm thanh: {filePath}");
                }

                var outputDevice = new WaveOutEvent();
                var audioFile = CreateAudioReader(filePath);

                // Chỉ set volume nếu là AudioFileReader
                if (audioFile is AudioFileReader audioReader)
                {
                    audioReader.Volume = _effectVolume;
                }

                outputDevice.Init(audioFile);
                outputDevice.PlaybackStopped += (s, e) =>
                {
                    outputDevice.Dispose();
                    audioFile.Dispose();
                };
                outputDevice.Play();
            }
            catch (FileNotFoundException ex)
            {
                LogError("Phát effect trực tiếp", ex);
                MessageBox.Show($"Không tìm thấy file effect: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LogError("Phát effect trực tiếp", ex);
            }
        }

        /// <summary>
        /// Xóa effect khỏi bộ nhớ
        /// </summary>
        public void UnloadEffect(string effectName)
        {
            try
            {
                if (_effectSounds.ContainsKey(effectName))
                {
                    _effectSounds[effectName].Dispose();
                    _effectSounds.Remove(effectName);
                }
            }
            catch (Exception ex)
            {
                LogError($"Xóa effect '{effectName}'", ex);
            }
        }

        /// <summary>
        /// Xóa tất cả effects khỏi bộ nhớ
        /// </summary>
        public void UnloadAllEffects()
        {
            try
            {
                foreach (var sound in _effectSounds.Values)
                {
                    sound?.Dispose();
                }
                _effectSounds.Clear();
            }
            catch (Exception ex)
            {
                LogError("Xóa tất cả effects", ex);
            }
        }

        /// <summary>
        /// Âm lượng effect (0.0 - 1.0)
        /// </summary>
        public float EffectVolume
        {
            get => _effectVolume;
            set => _effectVolume = Math.Max(0f, Math.Min(1f, value));
        }

        #endregion

        #region Helper Methods

        private WaveStream CreateAudioReader(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            // Đối với OGG, sử dụng VorbisWaveReader
            if (extension == ".ogg")
            {
                return new VorbisWaveReader(filePath);
            }

            // MP3, WAV, FLAC, AIFF được xử lý tự động bởi AudioFileReader
            return new AudioFileReader(filePath);
        }

        private void PlayCachedSound(CachedSound sound)
        {
            var outputDevice = new WaveOutEvent();
            var provider = new CachedSoundSampleProvider(sound);
            var volumeProvider = new VolumeSampleProvider(provider);
            volumeProvider.Volume = _effectVolume;

            outputDevice.Init(volumeProvider);
            outputDevice.PlaybackStopped += (s, e) =>
            {
                outputDevice.Dispose();
            };
            outputDevice.Play();
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Giải phóng tất cả tài nguyên
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopBackground();
                UnloadAllEffects();
            }
            catch (Exception ex)
            {
                LogError("Dispose SoundManager", ex);
            }
        }

        #endregion

        #region Error Logging

        private void LogError(string action, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SoundManager Error] {action}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Lưu trữ âm thanh trong bộ nhớ để phát nhanh
    /// </summary>
    internal class CachedSound : IDisposable
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }

        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }

        public void Dispose()
        {
            AudioData = null;
        }
    }

    /// <summary>
    /// Provider để phát cached sound
    /// </summary>
    internal class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _cachedSound;
        private long _position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            _cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = _cachedSound.AudioData.Length - _position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat => _cachedSound.WaveFormat;
    }

    #endregion
}
