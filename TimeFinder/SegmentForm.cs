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

public class SegmentForm : Form
{
    private TextBox nameTextBox;
    private ComboBox defaultNextComboBox;
    private Button startTimeButton;
    private Button endTimeButton;
    private Button saveButton;
    private CheckBox choicePointCheckBox;
    private Button interactionStartTimeButton;
    private Button interactionEndTimeButton;
    private TextBox startTimeTextBox;
    private TextBox endTimeTextBox;
    private TextBox interactionStartTimeTextBox;
    private TextBox interactionEndTimeTextBox;
    private Dictionary<string, Segment> segments;
    private string segmentName;
    private long startTimeMs;
    private long endTimeMs;
    private long interactionStartTimeMs;
    private long interactionEndTimeMs;
    private string defaultFolderPath;
    private long lastPlayedTime;
    private ListBox choicesListBox;
    private Button addChoiceButton;
    private Button removeChoiceButton;
    private Button copyChoicesButton; // New button
    private List<string> variableNames;

    public SegmentForm(Dictionary<string, Segment> segments, string segmentName = null, string defaultVideoPath = "", long lastPlayedTime = 0, List<string> variableNames = null)
    {
        this.Icon = new Icon("icon.ico");
        this.segments = segments;
        this.segmentName = segmentName;
        this.defaultFolderPath = defaultVideoPath;
        this.lastPlayedTime = lastPlayedTime;
        this.variableNames = variableNames ?? new List<string>();
        InitializeComponents();

        if (segmentName != null)
        {
            LoadSegmentData();
        }
    }

    private void InitializeComponents()
    {
        this.Text = "Segment Editor";
        this.Width = 350;
        this.Height = 450;

        System.Windows.Forms.Label nameLabel = new System.Windows.Forms.Label { Text = "Name:", Top = 10, Left = 10 };
        nameTextBox = new TextBox { Top = 10, Left = 120, Width = 150 };

        System.Windows.Forms.Label defaultNextLabel = new System.Windows.Forms.Label { Text = "Default Next:", Top = 40, Left = 10 };
        defaultNextComboBox = new ComboBox { Top = 40, Left = 120, Width = 150 };

        startTimeButton = new Button { Text = "Set Start Time", Top = 70, Left = 10 };
        startTimeButton.Click += StartTimeButton_Click;

        startTimeTextBox = new TextBox { Top = 70, Left = 120, Width = 150 };

        endTimeButton = new Button { Text = "Set End Time", Top = 100, Left = 10 };
        endTimeButton.Click += EndTimeButton_Click;

        endTimeTextBox = new TextBox { Top = 100, Left = 120, Width = 150 };

        choicePointCheckBox = new CheckBox { Text = "Choice Point", Top = 130, Left = 10 };
        choicePointCheckBox.CheckedChanged += ChoicePointCheckBox_CheckedChanged;

        interactionStartTimeButton = new Button { Text = "Set Start", Top = 160, Left = 10, Enabled = false };
        interactionStartTimeButton.Click += InteractionStartTimeButton_Click;

        interactionStartTimeTextBox = new TextBox { Top = 160, Left = 180, Width = 150, Enabled = false };

        interactionEndTimeButton = new Button { Text = "Set End", Top = 190, Left = 10, Enabled = false };
        interactionEndTimeButton.Click += InteractionEndTimeButton_Click;

        interactionEndTimeTextBox = new TextBox { Top = 190, Left = 180, Width = 150, Enabled = false };

        choicesListBox = new ListBox { Top = 220, Left = 10, Width = 320, Height = 100, Enabled = false };
        choicesListBox.DoubleClick += ChoicesListBox_DoubleClick;

        addChoiceButton = new Button { Text = "Add Choice", Top = 330, Left = 10, Enabled = false };
        addChoiceButton.Click += AddChoiceButton_Click;

        removeChoiceButton = new Button { Text = "Remove Choice", Top = 330, Left = 100, Enabled = false };
        removeChoiceButton.Click += RemoveChoiceButton_Click;

        copyChoicesButton = new Button { Text = "Copy Choices", Top = 330, Left = 200, Enabled = false }; // New button
        copyChoicesButton.Click += CopyChoicesButton_Click;

        saveButton = new Button { Text = "Save Segment", Top = 360, Left = 10 };
        saveButton.Click += SaveButton_Click;

        this.Controls.Add(nameLabel);
        this.Controls.Add(nameTextBox);
        this.Controls.Add(defaultNextLabel);
        this.Controls.Add(defaultNextComboBox);
        this.Controls.Add(startTimeButton);
        this.Controls.Add(startTimeTextBox);
        this.Controls.Add(endTimeButton);
        this.Controls.Add(endTimeTextBox);
        this.Controls.Add(choicePointCheckBox);
        this.Controls.Add(interactionStartTimeButton);
        this.Controls.Add(interactionStartTimeTextBox);
        this.Controls.Add(interactionEndTimeButton);
        this.Controls.Add(interactionEndTimeTextBox);
        this.Controls.Add(choicesListBox);
        this.Controls.Add(addChoiceButton);
        this.Controls.Add(removeChoiceButton);
        this.Controls.Add(copyChoicesButton); // Add the new button to the form
        this.Controls.Add(saveButton);

        UpdateDefaultNextComboBox();
    }

    private void CopyChoicesButton_Click(object sender, EventArgs e)
    {
        using (var copyChoicesForm = new CopyChoicesForm(segments.Keys.ToList()))
        {
            if (copyChoicesForm.ShowDialog() == DialogResult.OK)
            {
                var selectedSegmentName = copyChoicesForm.SelectedSegmentName;
                if (segments.TryGetValue(selectedSegmentName, out var selectedSegment))
                {
                    choicesListBox.Items.Clear();
                    foreach (var choice in selectedSegment.choices)
                    {
                        choicesListBox.Items.Add($"{choice.text} -> {choice.segmentId}");
                    }
                }
            }
        }
    }

    public class CopyChoicesForm : Form
    {
        private ComboBox segmentComboBox;
        private Button copyButton;
        private Button cancelButton;

        public string SelectedSegmentName { get; private set; }

        public CopyChoicesForm(List<string> segmentNames)
        {
            InitializeComponents(segmentNames);
        }

        private void InitializeComponents(List<string> segmentNames)
        {
            this.Text = "Copy Choices";
            this.Width = 300;
            this.Height = 150;

            System.Windows.Forms.Label segmentLabel = new System.Windows.Forms.Label { Text = "Select Segment:", Top = 20, Left = 10 };
            segmentComboBox = new ComboBox { Top = 20, Left = 120, Width = 150 };
            segmentComboBox.Items.AddRange(segmentNames.ToArray());

            copyButton = new Button { Text = "Copy", Top = 60, Left = 50 };
            copyButton.Click += CopyButton_Click;

            cancelButton = new Button { Text = "Cancel", Top = 60, Left = 150 };
            cancelButton.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(segmentLabel);
            this.Controls.Add(segmentComboBox);
            this.Controls.Add(copyButton);
            this.Controls.Add(cancelButton);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (segmentComboBox.SelectedItem != null)
            {
                SelectedSegmentName = segmentComboBox.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please select a segment.");
            }
        }
    }

    private void LoadSegmentData()
    {
        if (segments.TryGetValue(segmentName, out var segment))
        {
            nameTextBox.Text = segmentName;
            defaultNextComboBox.SelectedItem = segment.defaultNext;
            startTimeMs = segment.startTimeMs;
            endTimeMs = segment.endTimeMs;

            if (segment.ui != null && segment.ui.interactionZones != null && segment.ui.interactionZones.Count > 0)
            {
                interactionStartTimeMs = segment.ui.interactionZones[0][0];
                interactionEndTimeMs = segment.ui.interactionZones[0][1];
                choicePointCheckBox.Checked = true;
            }
            else
            {
                interactionStartTimeMs = 0;
                interactionEndTimeMs = 0;
                choicePointCheckBox.Checked = false;
            }

            choicesListBox.Items.Clear();
            if (segment.choices != null)
            {
                foreach (var choice in segment.choices)
                {
                    choicesListBox.Items.Add($"{choice.text} -> {choice.segmentId}");
                }
            }

            UpdateTimeTextBoxes();
        }
        else
        {
            MessageBox.Show("Segment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StartTimeButton_Click(object sender, EventArgs e)
    {
        using (var mediaPlayerForm = new MediaPlayerForm(defaultFolderPath, lastPlayedTime))
        {
            if (mediaPlayerForm.ShowDialog() == DialogResult.OK)
            {
                startTimeMs = mediaPlayerForm.SelectedTime;
                UpdateTimeTextBoxes();
            }
        }
    }

    private void EndTimeButton_Click(object sender, EventArgs e)
    {
        using (var mediaPlayerForm = new MediaPlayerForm(defaultFolderPath, lastPlayedTime))
        {
            if (mediaPlayerForm.ShowDialog() == DialogResult.OK)
            {
                endTimeMs = mediaPlayerForm.SelectedTime;
                UpdateTimeTextBoxes();
            }
        }
    }

    private void InteractionStartTimeButton_Click(object sender, EventArgs e)
    {
        using (var mediaPlayerForm = new MediaPlayerForm(defaultFolderPath, lastPlayedTime))
        {
            if (mediaPlayerForm.ShowDialog() == DialogResult.OK)
            {
                interactionStartTimeMs = mediaPlayerForm.SelectedTime;
                UpdateTimeTextBoxes();
            }
        }
    }

    private void InteractionEndTimeButton_Click(object sender, EventArgs e)
    {
        using (var mediaPlayerForm = new MediaPlayerForm(defaultFolderPath, lastPlayedTime))
        {
            if (mediaPlayerForm.ShowDialog() == DialogResult.OK)
            {
                interactionEndTimeMs = mediaPlayerForm.SelectedTime;
                UpdateTimeTextBoxes();
            }
        }
    }

    private void ChoicePointCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        bool isChecked = choicePointCheckBox.Checked;
        interactionStartTimeButton.Enabled = isChecked;
        interactionStartTimeTextBox.Enabled = isChecked;
        interactionEndTimeButton.Enabled = isChecked;
        interactionEndTimeTextBox.Enabled = isChecked;
        choicesListBox.Enabled = isChecked;
        addChoiceButton.Enabled = isChecked;
        removeChoiceButton.Enabled = isChecked;
        copyChoicesButton.Enabled = isChecked;
    }

    private void AddChoiceButton_Click(object sender, EventArgs e)
    {
        using (var addChoiceForm = new AddChoiceForm(segments.Keys.ToList(), variableNames))
        {
            if (addChoiceForm.ShowDialog() == DialogResult.OK)
            {
                var choice = addChoiceForm.Choice;
                choicesListBox.Items.Add($"{choice.text} -> {choice.segmentId}");
                if (segments[segmentName].choices == null)
                {
                    segments[segmentName].choices = new List<Choice>();
                }
                segments[segmentName].choices.Add(choice);
            }
        }
    }

    private void ChoicesListBox_DoubleClick(object sender, EventArgs e)
    {
        if (choicesListBox.SelectedItem != null)
        {
            var selectedItem = choicesListBox.SelectedItem.ToString();
            var parts = selectedItem.Split(new string[] { " -> " }, StringSplitOptions.None);
            var choice = segments[segmentName].choices.FirstOrDefault(c => c.text == parts[0] && c.segmentId == parts[1]);

            if (choice != null)
            {
                using (var addChoiceForm = new AddChoiceForm(segments.Keys.ToList(), variableNames, choice))
                {
                    if (addChoiceForm.ShowDialog() == DialogResult.OK)
                    {
                        var editedChoice = addChoiceForm.Choice;
                        choicesListBox.Items[choicesListBox.SelectedIndex] = $"{editedChoice.text} -> {editedChoice.segmentId}";
                        segments[segmentName].choices[choicesListBox.SelectedIndex] = editedChoice;
                    }
                }
            }
        }
    }

    private void RemoveChoiceButton_Click(object sender, EventArgs e)
    {
        if (choicesListBox.SelectedItem != null)
        {
            choicesListBox.Items.Remove(choicesListBox.SelectedItem);
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        string name = nameTextBox.Text;
        string defaultNext = defaultNextComboBox.SelectedItem?.ToString();

        if (!string.IsNullOrEmpty(name))
        {
            if (long.TryParse(startTimeTextBox.Text, out startTimeMs) && long.TryParse(endTimeTextBox.Text, out endTimeMs))
            {
                var segment = new Segment
                {
                    startTimeMs = startTimeMs,
                    endTimeMs = endTimeMs,
                    defaultNext = defaultNext,
                    choices = new List<Choice>(),
                    ui = new UI { interactionZones = new List<long[]>() }
                };

                if (choicePointCheckBox.Checked)
                {
                    if (long.TryParse(interactionStartTimeTextBox.Text, out interactionStartTimeMs) && long.TryParse(interactionEndTimeTextBox.Text, out interactionEndTimeMs))
                    {
                        segment.ui.interactionZones.Add(new long[] { interactionStartTimeMs, interactionEndTimeMs });
                    }

                    foreach (var item in choicesListBox.Items)
                    {
                        var parts = item.ToString().Split(new string[] { " -> " }, StringSplitOptions.None);
                        var choice = segments[segmentName]?.choices?.FirstOrDefault(c => c.text == parts[0] && c.segmentId == parts[1]);
                        if (choice != null)
                        {
                            segment.choices.Add(choice);
                        }
                        else
                        {
                            segment.choices.Add(new Choice { text = parts[0], segmentId = parts[1] });
                        }
                    }
                }

                if (segment.ui.interactionZones.Count == 0)
                {
                    segment.ui = null;
                }

                segments[name] = segment;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid time values");
            }
        }
        else
        {
            MessageBox.Show("Segment name cannot be empty");
        }
    }

    private void UpdateTimeTextBoxes()
    {
        startTimeTextBox.Text = startTimeMs.ToString();
        endTimeTextBox.Text = endTimeMs.ToString();
        interactionStartTimeTextBox.Text = interactionStartTimeMs.ToString();
        interactionEndTimeTextBox.Text = interactionEndTimeMs.ToString();
    }

    private void UpdateDefaultNextComboBox()
    {
        defaultNextComboBox.Items.Clear();
        foreach (var segment in segments.Keys)
        {
            defaultNextComboBox.Items.Add(segment);
        }
    }
}
