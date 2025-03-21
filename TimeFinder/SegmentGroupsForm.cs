using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class SegmentGroupsForm : Form
{
    private Dictionary<string, List<SegmentGroupItem>> segmentGroups;
    private ListBox segmentGroupsListBox;
    private TextBox groupNameTextBox;
    private ComboBox defaultSegmentComboBox;
    private ComboBox segmentComboBox;
    private ComboBox preconditionComboBox;
    private ListBox preconditionSegmentsListBox;
    private Button addGroupButton;
    private Button removeGroupButton;
    private Button addPreconditionSegmentButton;
    private Button removePreconditionSegmentButton;
    private Button saveButton;

    public SegmentGroupsForm(Dictionary<string, List<SegmentGroupItem>> segmentGroups, List<string> segmentNames, List<string> preconditionNames)
    {
        this.Icon = new Icon("icon.ico");
        this.segmentGroups = segmentGroups;
        InitializeComponents(segmentNames, preconditionNames);
    }

    private void InitializeComponents(List<string> segmentNames, List<string> preconditionNames)
    {
        this.Text = "Manage Segment Groups";
        this.Width = 600;
        this.Height = 500;

        Label groupNameLabel = new Label { Text = "Group Name:", Top = 10, Left = 10 };
        groupNameTextBox = new TextBox { Top = 10, Left = 150, Width = 200 };

        Label defaultSegmentLabel = new Label { Text = "Default Segment:", Top = 40, Left = 10 };
        defaultSegmentComboBox = new ComboBox { Top = 40, Left = 150, Width = 200 };
        defaultSegmentComboBox.Items.AddRange(segmentNames.ToArray());

        Label segmentLabel = new Label { Text = "Segment:", Top = 70, Left = 10 };
        segmentComboBox = new ComboBox { Top = 70, Left = 150, Width = 200 };
        segmentComboBox.Items.AddRange(segmentNames.ToArray());

        Label preconditionLabel = new Label { Text = "Precondition:", Top = 100, Left = 10 };
        preconditionComboBox = new ComboBox { Top = 100, Left = 150, Width = 200 };
        preconditionComboBox.Items.AddRange(preconditionNames.ToArray());

        Label preconditionSegmentsLabel = new Label { Text = "Precondition Segments:", Top = 130, Left = 10 };
        preconditionSegmentsListBox = new ListBox { Top = 130, Left = 150, Width = 200, Height = 100 };

        addPreconditionSegmentButton = new Button { Text = "Add", Top = 240, Left = 150 };
        addPreconditionSegmentButton.Click += AddPreconditionSegmentButton_Click;

        removePreconditionSegmentButton = new Button { Text = "Remove", Top = 240, Left = 250 };
        removePreconditionSegmentButton.Click += RemovePreconditionSegmentButton_Click;

        addGroupButton = new Button { Text = "Add Group", Top = 270, Left = 150 };
        addGroupButton.Click += AddGroupButton_Click;

        removeGroupButton = new Button { Text = "Remove Group", Top = 270, Left = 250 };
        removeGroupButton.Click += RemoveGroupButton_Click;

        saveButton = new Button { Text = "Save", Top = 270, Left = 350 };
        saveButton.Click += SaveButton_Click;

        segmentGroupsListBox = new ListBox { Top = 300, Left = 10, Width = 560, Height = 150 };
        UpdateSegmentGroupsListBox();

        this.Controls.Add(groupNameLabel);
        this.Controls.Add(groupNameTextBox);
        this.Controls.Add(defaultSegmentLabel);
        this.Controls.Add(defaultSegmentComboBox);
        this.Controls.Add(segmentLabel);
        this.Controls.Add(segmentComboBox);
        this.Controls.Add(preconditionLabel);
        this.Controls.Add(preconditionComboBox);
        this.Controls.Add(preconditionSegmentsLabel);
        this.Controls.Add(preconditionSegmentsListBox);
        this.Controls.Add(addPreconditionSegmentButton);
        this.Controls.Add(removePreconditionSegmentButton);
        this.Controls.Add(addGroupButton);
        this.Controls.Add(removeGroupButton);
        this.Controls.Add(saveButton);
        this.Controls.Add(segmentGroupsListBox);
    }

    private void AddPreconditionSegmentButton_Click(object sender, EventArgs e)
    {
        string segment = segmentComboBox.SelectedItem?.ToString();
        string precondition = preconditionComboBox.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(segment) && !string.IsNullOrEmpty(precondition))
        {
            preconditionSegmentsListBox.Items.Add(new SegmentGroupItem { Segment = segment, Precondition = precondition });
        }
    }

    private void RemovePreconditionSegmentButton_Click(object sender, EventArgs e)
    {
        if (preconditionSegmentsListBox.SelectedItem != null)
        {
            preconditionSegmentsListBox.Items.Remove(preconditionSegmentsListBox.SelectedItem);
        }
    }

    private void AddGroupButton_Click(object sender, EventArgs e)
    {
        string groupName = groupNameTextBox.Text;
        string defaultSegment = defaultSegmentComboBox.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(defaultSegment))
        {
            var preconditionSegments = new List<SegmentGroupItem>();
            foreach (var item in preconditionSegmentsListBox.Items)
            {
                preconditionSegments.Add((SegmentGroupItem)item);
            }
            preconditionSegments.Add(new SegmentGroupItem { Segment = defaultSegment });

            segmentGroups[groupName] = preconditionSegments;
            UpdateSegmentGroupsListBox();
        }
    }

    private void RemoveGroupButton_Click(object sender, EventArgs e)
    {
        if (segmentGroupsListBox.SelectedItem != null)
        {
            string groupName = segmentGroupsListBox.SelectedItem.ToString().Split(':')[0];
            segmentGroups.Remove(groupName);
            UpdateSegmentGroupsListBox();
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void UpdateSegmentGroupsListBox()
    {
        segmentGroupsListBox.Items.Clear();
        foreach (var group in segmentGroups)
        {
            segmentGroupsListBox.Items.Add($"{group.Key}: {string.Join(", ", group.Value)}");
        }
    }

    public Dictionary<string, List<SegmentGroupItem>> GetSegmentGroups()
    {
        return segmentGroups;
    }
}

public class SegmentGroupItem
{
    public string Segment { get; set; }
    public string Precondition { get; set; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Precondition) ? Segment : $"{Segment} (Precondition: {Precondition})";
    }
}
