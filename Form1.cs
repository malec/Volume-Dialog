using System.Windows.Forms;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;

namespace VolumeDialog___Windows_Forms {
    public partial class VolumeForm : Form {
        delegate void setVolumeCallback();
        private Queue<int> volumeQueue;
        public VolumeForm() {
            InitializeComponent();
            volumeQueue = new Queue<int>();
            var enumer = new MMDeviceEnumerator();
            var device = enumer.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var currentVolume = (int)(100*device.AudioEndpointVolume.MasterVolumeLevelScalar);
            if (volumeInRange(currentVolume)) {
                progressBar1.Value = currentVolume;
                label1.Text = currentVolume.ToString();
            }
            else {
                throw new IndexOutOfRangeException("Volume in a bad state. Naudio thinks volume is at " + currentVolume.ToString());
            }
            device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            hideTimer.Start();
        }
        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) {
            int volume = Convert.ToInt32(Math.Floor(100 * data.MasterVolume));
            if (volumeInRange(volume)) {
                volumeQueue.Enqueue(volume);
                if (progressBar1.InvokeRequired) {
                    setVolumeCallback cb = new setVolumeCallback(setPBarValue);
                    this.Invoke(cb);
                }
                else {
                    setPBarValue();
                }
            }
            else {
                throw new IndexOutOfRangeException("Bad volume. Data is " + volume);
            }
        }
        private void setPBarValue() {
            Opacity = 100;
            Show();
            BringToFront();
            int volume = volumeQueue.Dequeue();
            label1.Text = volume.ToString();
            hideTimer.Stop();
            progressBar1.Value = volume;
            hideTimer.Start();
        }

        private void hideTimer_Tick(object sender, EventArgs e) {
            fadeOutTimer.Start();
        }

        private bool volumeInRange(int volume) {
            if (volume >= 0 && volume <= 100) {
                return true;
            }
            return false;
        }

        private void fadeTimer_Tick(object sender, EventArgs e) {
            if (Opacity >= .05) {
                Opacity -= .05;
            }
            else {
                Hide();
                fadeOutTimer.Stop();
            }
        }
    }
}
