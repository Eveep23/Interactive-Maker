using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

public class MediaPlayerForm : Form
{
    private MediaPlayer mediaPlayer;
    private LibVLC libVLC;
    private Button playPauseButton;
    private Button forwardButton;
    private Button backwardButton;
    private Button forward40msButton;
    private Button backward40msButton;
    private Button lockTimeButton;
    private System.Windows.Forms.Label currentTimeLabel;
    public long SelectedTime { get; private set; }
    private bool isPaused = false;
    private long lastPlayedTime;
    private string defaultFolderPath;

    public MediaPlayerForm(string defaultFolderPath, long lastPlayedTime)
    {
        this.Icon = new Icon("icon.ico");
        this.defaultFolderPath = defaultFolderPath;
        this.lastPlayedTime = lastPlayedTime;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Media Player";
        this.Width = 750;
        this.Height = 500;

        libVLC = new LibVLC();
        mediaPlayer = new MediaPlayer(libVLC);

        var videoView = new LibVLCSharp.WinForms.VideoView { MediaPlayer = mediaPlayer, Dock = DockStyle.Top, Height = 400 };
        this.Controls.Add(videoView);

        playPauseButton = new Button { Text = "Play", Top = 410, Left = 10 };
        playPauseButton.Click += PlayPauseButton_Click;

        backwardButton = new Button { Text = "Backward", Top = 410, Left = 90 };
        backwardButton.Click += BackwardButton_Click;

        forwardButton = new Button { Text = "Forward", Top = 410, Left = 170 };
        forwardButton.Click += ForwardButton_Click;

        backward40msButton = new Button { Text = "Last Frame", Top = 410, Left = 250 };
        backward40msButton.Click += Backward40msButton_Click;

        forward40msButton = new Button { Text = "Next Frame", Top = 410, Left = 330 };
        forward40msButton.Click += Forward40msButton_Click;

        lockTimeButton = new Button { Text = "Lock Time", Top = 410, Left = 410 };
        lockTimeButton.Click += LockTimeButton_Click;

        currentTimeLabel = new System.Windows.Forms.Label { Text = "Current Time: 0 ms", Top = 410, Left = 490, Width = 200 };

        this.Controls.Add(playPauseButton);
        this.Controls.Add(backwardButton);
        this.Controls.Add(forwardButton);
        this.Controls.Add(backward40msButton);
        this.Controls.Add(forward40msButton);
        this.Controls.Add(lockTimeButton);
        this.Controls.Add(currentTimeLabel);

        this.FormClosing += MediaPlayerForm_FormClosing;
    }

    private void PlayPauseButton_Click(object sender, EventArgs e)
    {
        if (isPaused)
        {
            mediaPlayer.Play();
            playPauseButton.Text = "Pause";
        }
        else
        {
            mediaPlayer.Pause();
            playPauseButton.Text = "Play";
        }
        isPaused = !isPaused;
    }

    private void ForwardButton_Click(object sender, EventArgs e)
    {
        if (isPaused)
        {
            mediaPlayer.Time += 1; // Move forward 1 millisecond
        }
        else
        {
            mediaPlayer.Time += 10000; // Move forward 10 seconds
        }
        UpdateCurrentTimeLabel();
    }

    private void BackwardButton_Click(object sender, EventArgs e)
    {
        if (isPaused)
        {
            mediaPlayer.Time = Math.Max(0, mediaPlayer.Time - 1); // Move backward 1 millisecond
        }
        else
        {
            mediaPlayer.Time = Math.Max(0, mediaPlayer.Time - 10000); // Move backward 10 seconds
        }
        UpdateCurrentTimeLabel();
    }

    private void Forward40msButton_Click(object sender, EventArgs e)
    {
        mediaPlayer.Time += 40; // Move forward 40 milliseconds
        UpdateCurrentTimeLabel();
    }

    private void Backward40msButton_Click(object sender, EventArgs e)
    {
        mediaPlayer.Time = Math.Max(0, mediaPlayer.Time - 40); // Move backward 40 milliseconds
        UpdateCurrentTimeLabel();
    }

    private void LockTimeButton_Click(object sender, EventArgs e)
    {
        SelectedTime = mediaPlayer.Time;
        SaveLastPlayedTime(SelectedTime);
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void SaveLastPlayedTime(long time)
    {
        var settings = new
        {
            DefaultFolderPath = defaultFolderPath,
            LastPlayedTime = time
        };

        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        System.IO.File.WriteAllText("settings.json", json);
    }

    private void UpdateCurrentTimeLabel()
    {
        currentTimeLabel.Text = $"Current Time: {mediaPlayer.Time} ms";
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        LoadSettings();

        if (!string.IsNullOrEmpty(defaultFolderPath))
        {
            var mkvFiles = System.IO.Directory.GetFiles(defaultFolderPath, "*.mkv");
            if (mkvFiles.Length > 0)
            {
                var media = new Media(libVLC, mkvFiles[0], FromType.FromPath);
                mediaPlayer.Media = media;
                mediaPlayer.Play();
                mediaPlayer.Time = lastPlayedTime;
                playPauseButton.Text = "Pause";
                isPaused = false;
            }
            else
            {
                MessageBox.Show("No video files found in the default folder path.");
                this.Close();
            }
        }
        else
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var media = new Media(libVLC, openFileDialog.FileName, FromType.FromPath);
                    mediaPlayer.Media = media;
                    mediaPlayer.Play();
                    playPauseButton.Text = "Pause";
                    isPaused = false;
                }
                else
                {
                    this.Close();
                }
            }
        }
    }

    private void LoadSettings()
    {
        if (System.IO.File.Exists("settings.json"))
        {
            string json = System.IO.File.ReadAllText("settings.json");
            var settings = JsonConvert.DeserializeObject<dynamic>(json);
            defaultFolderPath = settings.DefaultFolderPath;
            lastPlayedTime = settings.LastPlayedTime;
        }
        else
        {
            lastPlayedTime = 0; // Default to 0 if settings file does not exist
        }
    }

    private void MediaPlayerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        mediaPlayer.Dispose();
        libVLC.Dispose();
    }
}
