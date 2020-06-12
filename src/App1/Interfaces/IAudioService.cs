using System;
using System.Collections.Generic;

namespace App1
{
    public interface IAudioService
    {
        void PlaySound(Queue<Byte[]> audio);

        void PlaySound(Byte[] audio);
        void StopSound();
    }
}