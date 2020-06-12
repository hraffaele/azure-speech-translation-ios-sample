using AVFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;

namespace App1.iOS.Services
{
    public class AudioService : IAudioService
    {
        private AVAudioPlayer _player;
        public AudioService()
        {

        }

        public void PlaySound(byte[] audio)
        {
            var queue = new Queue<byte[]>();
            queue.Enqueue(audio);
            PlaySound(queue);
        }

        public void PlaySound(Queue<byte[]> audio)
        {
            if (audio == null)
            {
                throw new ApplicationException("audio is null");
            }

            if (audio.Count > 0)
            {
                var audioSource = audio.Dequeue();

                SetAudioSettings();
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Audio.wav");

                File.WriteAllBytes(filePath, (byte[])audioSource);

                _player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(filePath));

                if (_player == null)
                {
                    throw new ApplicationException("_player is null");
                }
                
                _player.FinishedPlaying += (sender, e) =>
                {
                    PlaySound(audio);
                };
                
                _player.Play();               
            }
        }

        void SetAudioSettings()
        {
            NSError error = AVFoundation.AVAudioSession.SharedInstance().SetCategory(AVFoundation.AVAudioSessionCategory.PlayAndRecord, AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker);

            if (error == null)
            {
                if (AVFoundation.AVAudioSession.SharedInstance().SetMode(AVFoundation.AVAudioSession.ModeVideoChat, out error))
                {
                    if (AVFoundation.AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVFoundation.AVAudioSessionPortOverride.Speaker, out error))
                    {
                        error = AVFoundation.AVAudioSession.SharedInstance().SetActive(true);

                        if (error != null)
                        {
                            //Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set active"));
                        }
                    }
                    else
                    {
                        //Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot override output audio port"));
                    }
                }
                else
                {
                    //Logger.Log(new Exception("Cannot set mode"));
                }
            }
            else
            {
                //Logger.Log(new Exception(error?.LocalizedDescription ?? "Cannot set category"));
            }
        }

        public void StopSound()
        {
            if (_player != null)
            {
                _player.Stop();
            }
        }
    }
}