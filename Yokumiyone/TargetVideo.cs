using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;


namespace Yokumiyone
{
    class TargetVideo
    {
        private MediaState m_stateCurrent;
        private MediaElement movie = new MediaElement();
        private Slider progressSlider = new Slider();
        private Button playPause = new Button();
        private Button progressLabel = new Button();
        private double lastSliderValue;
        private DispatcherTimer m_timer = new DispatcherTimer();
        private SkipPlayControl skipPlayControl = new SkipPlayControl();
        private CruisePlayControl cruisePlayControl = new CruisePlayControl();

        private int recentSkipPos = 0;

        public double TotalSec
        {
            get { return movie.NaturalDuration.TimeSpan.TotalSeconds; }
        }

        public void SetControl(Slider progressSlider, Button playPauseButton, Button progressButton, SkipPlayControl skipPlayControl, CruisePlayControl cruisePlayControl)
        {
            this.progressSlider = progressSlider;
            this.progressSlider.Maximum = 1000;
            this.progressLabel = progressButton;
            playPause = playPauseButton;
            this.skipPlayControl = skipPlayControl;
            this.cruisePlayControl = cruisePlayControl;
        }
        public void SetMediaElement(MediaElement mediaElement)
        {
            movie = mediaElement;

            // タイマー設定
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += DispatcherTimer_Tick;

            // 動画読み込み直後はRabbitモードOFF
            this.skipPlayControl.IsSkipMode = false;
        }

        public void Rotate(double angle)
        {
            movie.LayoutTransform = new RotateTransform(angle);
        }

        public void TimerStart()
        {
            if (m_timer != null)
            {
                m_timer.Start();
            }
        }

        public void TimerStop()
        {
            if (m_timer != null)
            {
                m_timer.Stop();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsMovieEnable() == false)
            {
                return;
            }
            SyncSliderAndSeek();
            string position = movie.Position.ToString() ?? "00:00:00.000";
            if (position.Length > 10)
            {
                position = position.Substring(0, 12);
            }
            progressLabel.Content = position;

            // Rabbitモード
            if (this.skipPlayControl.IsSkipMode.Value == true)
            {
                int nowPos = (int)movie.Position.TotalSeconds;
                if ((nowPos - recentSkipPos) >= this.skipPlayControl.PlaySec)
                {
                    Step(1000 * (this.skipPlayControl.SkipSec));
                    recentSkipPos = nowPos + this.skipPlayControl.SkipSec;
                }
                else if ((nowPos - recentSkipPos) < 0)
                {
                    recentSkipPos = nowPos;
                }
            }
            else
            {
                recentSkipPos = 0;
            }

            // Birdモード
            if (this.cruisePlayControl.IsCruiseMode == true)
            {
                TimeSpan nowPos = movie.Position;
                if (this.cruisePlayControl.IsInScene(nowPos) == false)
                {
                    TimeSpan nextPos = this.cruisePlayControl.NextScene(nowPos);
                    this.Jump(nextPos);
                }
            }

            if (this.cruisePlayControl.IsFastMode == true)
            {
                movie.SpeedRatio = 3.0;
            }
            else
            {
                movie.SpeedRatio = 1.0;
            }

        }

        public MediaState GetState()
        {
            return m_stateCurrent;
        }

        private void SyncSliderAndSeek()
        {
            try
            {
                if (m_stateCurrent != MediaState.Play && m_stateCurrent != MediaState.Pause)
                {
                    return;
                }
                if (lastSliderValue == this.progressSlider.Value)
                {
                    // 動画経過時間に合わせてスライダーを動かす
                    double dbPrg = GetMovieProgressPercent();
                    this.progressSlider.Value = dbPrg * this.progressSlider.Maximum;
                    lastSliderValue = this.progressSlider.Value;
                    if (GetMediaState() == MediaState.Pause && m_stateCurrent == MediaState.Play)
                    {
                        movie.Play();
                    }
                }
                else
                {
                    // Sliderを操作したとき
                    if (m_stateCurrent == MediaState.Play)
                    {
                        movie.Pause();
                    }
                    // スライダーを動かした位置に合わせて動画の再生箇所を更新する
                    if (IsMovieEnable() == false)
                    {
                        return;
                    }
                    double dbSliderValue = this.progressSlider.Value;
                    double dbDurationMS = movie.NaturalDuration.TimeSpan.TotalMilliseconds;
                    int nSetMS = (int)(dbSliderValue * dbDurationMS / this.progressSlider.Maximum);
                    movie.Position = TimeSpan.FromMilliseconds(nSetMS);
                    lastSliderValue = this.progressSlider.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }

        public void Step(int msec)
        {
            double dbSliderValue = this.progressSlider.Value;
            if (IsMovieEnable() == false)
            {
                return;
            }

            double newMs = movie.Position.TotalMilliseconds + msec;
            if (newMs < 0)
            {
                newMs = 0;
            }
            else if (newMs > movie.NaturalDuration.TimeSpan.TotalMilliseconds)
            {
                newMs = movie.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
            Jump(newMs);
        }

        public void Jump(TimeSpan time)
        {
            double mseconds = time.TotalMilliseconds;
            Jump(mseconds);
        }

        public void Jump(double mseconds)
        {
            if (IsMovieEnable() == false)
            {
                return;
            }

            double dbDurationMs = movie.NaturalDuration.TimeSpan.TotalMilliseconds;
            this.progressSlider.Value = (int)(mseconds / dbDurationMs * this.progressSlider.Maximum);
            movie.Position = TimeSpan.FromMilliseconds(mseconds);
            lastSliderValue = this.progressSlider.Value;
        }

        // ステータス取得
        private MediaState GetMediaState()
        {
            MediaState state = new MediaState();
            if (movie == null)
            {
                return state;
            }
            System.Reflection.FieldInfo hlp = typeof(MediaElement).GetField("_helper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object helperObject = hlp.GetValue(movie);
            System.Reflection.FieldInfo stateField = helperObject.GetType().GetField("_currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        // 再生位置取得
        public double GetMovieProgressMs()
        {
            double dbPrg = 0;
            if (IsMovieEnable() == false)
            {
                return dbPrg;
            }
            TimeSpan tsCrnt = movie.Position;
            dbPrg = tsCrnt.TotalMilliseconds;
            return dbPrg;
        }

        // 再生位置取得
        private double GetMovieProgressPercent()
        {
            double dbPrg = 0;
            if (IsMovieEnable() == false)
            {
                return dbPrg;
            }
            TimeSpan tsCrnt = movie.Position;
            TimeSpan tsDuration = movie.NaturalDuration.TimeSpan;
            dbPrg = tsCrnt.TotalMilliseconds / tsDuration.TotalMilliseconds;
            return dbPrg;
        }


        public void Pause()
        {
            if (IsMovieEnable() == false)
            {
                return;
            }
            movie.Pause();
            this.skipPlayControl.IsSkipMode = false;
            this.cruisePlayControl.IsFastMode = false;
            m_stateCurrent = MediaState.Pause;
        }
        public void Play()
        {
            if (movie == null)
            {
                return;
            }
            movie.Play();
            m_stateCurrent = MediaState.Play;
        }

        public void Release()
        {
            if (movie == null || movie.Source == null)
            {
                return;
            }
            progressLabel.Content = "";
            this.progressSlider.Value = 0;
            movie.Source = null;
        }

        public bool IsMovieEnable()
        {
            bool res = false;
            if (movie == null || movie.NaturalDuration.HasTimeSpan == false)
            {
                res = false;
            }
            else
            {
                res = true;
            }
            return res;
        }
    }
}
