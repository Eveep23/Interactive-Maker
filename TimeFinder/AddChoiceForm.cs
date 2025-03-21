using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Drawing;

public class AddChoiceForm : Form
{
    private ComboBox segmentComboBox;
    private TextBox choiceTextBox;
    private Button okButton;
    private Button cancelButton;
    private ComboBox variableNameComboBox;
    private TextBox variableValueTextBox;
    private Button addVariableButton;
    private Button removeVariableButton; // New button to remove variable
    private ListBox variablesListBox;
    private Dictionary<string, object> impressionData = new Dictionary<string, object>();

    public Choice Choice { get; private set; }

    public AddChoiceForm(List<string> segmentNames, List<string> variableNames)
    {
        this.Icon = new Icon("icon.ico");
        InitializeComponents(segmentNames, variableNames);
    }

    public AddChoiceForm(List<string> segmentNames, List<string> variableNames, Choice choice) : this(segmentNames, variableNames)
    {
        segmentComboBox.SelectedItem = choice.segmentId;
        choiceTextBox.Text = choice.text;
        impressionData = new Dictionary<string, object>();

        if (choice.impressionData != null && choice.impressionData.ContainsKey("data"))
        {
            var data = choice.impressionData["data"] as JObject;
            if (data != null && data.ContainsKey("persistent"))
            {
                var persistentData = data["persistent"] as JObject;
                if (persistentData != null)
                {
                    foreach (var variable in persistentData)
                    {
                        impressionData[variable.Key] = variable.Value;
                    }
                }
            }
        }

        LoadVariables();
        Choice = choice;
    }

    private void InitializeComponents(List<string> segmentNames, List<string> variableNames)
    {
        this.Text = "Add Choice";
        this.Width = 300;
        this.Height = 400;

        System.Windows.Forms.Label segmentLabel = new System.Windows.Forms.Label { Text = "Segment:", Top = 20, Left = 10 };
        segmentComboBox = new ComboBox { Top = 20, Left = 120, Width = 150 };
        segmentComboBox.Items.AddRange(segmentNames.ToArray());

        System.Windows.Forms.Label choiceLabel = new System.Windows.Forms.Label { Text = "Choice Text:", Top = 60, Left = 10 };
        choiceTextBox = new TextBox { Top = 60, Left = 120, Width = 150 };

        System.Windows.Forms.Label variableNameLabel = new System.Windows.Forms.Label { Text = "Variable Name:", Top = 100, Left = 10 };
        variableNameComboBox = new ComboBox { Top = 100, Left = 120, Width = 150 };
        variableNameComboBox.Items.AddRange(variableNames.ToArray());

        System.Windows.Forms.Label variableValueLabel = new System.Windows.Forms.Label { Text = "Variable Value:", Top = 140, Left = 10 };
        variableValueTextBox = new TextBox { Top = 140, Left = 120, Width = 150 };

        addVariableButton = new Button { Text = "Add Variable", Top = 180, Left = 10 };
        addVariableButton.Click += AddVariableButton_Click;

        removeVariableButton = new Button { Text = "Remove Variable", Top = 180, Left = 120 }; // New button to remove variable
        removeVariableButton.Click += RemoveVariableButton_Click;

        variablesListBox = new ListBox { Top = 220, Left = 10, Width = 260, Height = 100 };

        okButton = new Button { Text = "OK", Top = 330, Left = 50 };
        okButton.Click += OkButton_Click;

        cancelButton = new Button { Text = "Cancel", Top = 330, Left = 150 };
        cancelButton.Click += (sender, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.Add(segmentLabel);
        this.Controls.Add(segmentComboBox);
        this.Controls.Add(choiceLabel);
        this.Controls.Add(choiceTextBox);
        this.Controls.Add(variableNameLabel);
        this.Controls.Add(variableNameComboBox);
        this.Controls.Add(variableValueLabel);
        this.Controls.Add(variableValueTextBox);
        this.Controls.Add(addVariableButton);
        this.Controls.Add(removeVariableButton); // Add the new button to the form
        this.Controls.Add(variablesListBox);
        this.Controls.Add(okButton);
        this.Controls.Add(cancelButton);
    }

    private void AddVariableButton_Click(object sender, EventArgs e)
    {
        string name = variableNameComboBox.SelectedItem?.ToString();
        string value = variableValueTextBox.Text;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
        {
            impressionData[name] = value;
            variablesListBox.Items.Add($"{name}: {value}");
            variableNameComboBox.SelectedIndex = -1;
            variableValueTextBox.Clear();
        }
        else
        {
            MessageBox.Show("Variable name and value cannot be empty");
        }
    }

    private void RemoveVariableButton_Click(object sender, EventArgs e)
    {
        if (variablesListBox.SelectedItem != null)
        {
            string selectedItem = variablesListBox.SelectedItem.ToString();
            string variableName = selectedItem.Split(':')[0].Trim();
            impressionData.Remove(variableName);
            variablesListBox.Items.Remove(selectedItem);
        }
        else
        {
            MessageBox.Show("Please select a variable to remove");
        }
    }

    private void LoadVariables()
    {
        variablesListBox.Items.Clear();
        foreach (var variable in impressionData)
        {
            variablesListBox.Items.Add($"{variable.Key}: {variable.Value}");
        }
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        if (segmentComboBox.SelectedItem != null && !string.IsNullOrEmpty(choiceTextBox.Text))
        {
            if (Choice == null)
            {
                Choice = new Choice();
            }
            Choice.segmentId = segmentComboBox.SelectedItem.ToString();
            Choice.text = choiceTextBox.Text;

            // Ensure impressionData is not nested repeatedly
            if (Choice.impressionData == null)
            {
                Choice.impressionData = new Dictionary<string, object>();
            }

            var persistentData = new JObject();
            if (Choice.impressionData.ContainsKey("data"))
            {
                var data = Choice.impressionData["data"] as JObject;
                if (data != null && data.ContainsKey("persistent"))
                {
                    persistentData = data["persistent"] as JObject;
                }
            }

            foreach (var variable in impressionData)
            {
                persistentData[variable.Key] = JToken.FromObject(variable.Value);
            }

            // Directly assign the persistent data to avoid nesting
            Choice.impressionData["data"] = new JObject { { "persistent", persistentData } };

            this.DialogResult = DialogResult.OK;
        }
        else
        {
            MessageBox.Show("Please select a segment and enter choice text.");
        }
    }
}
