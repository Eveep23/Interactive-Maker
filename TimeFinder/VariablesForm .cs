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

public class VariablesForm : Form
{
    private TextBox variableNameTextBox;
    private TextBox variableValueTextBox;
    private Button addVariableButton;
    private Button saveVariablesButton;
    private Button removeVariableButton;
    private ListBox variablesListBox;
    private Dictionary<string, object> variables;

    public VariablesForm(Dictionary<string, object> variables)
    {
        this.Icon = new Icon("icon.ico");
        this.variables = variables;
        InitializeComponents();
        LoadVariables();
    }

    private void InitializeComponents()
    {
        this.Text = "Manage Variables";
        this.Width = 400;
        this.Height = 300;

        System.Windows.Forms.Label variableNameLabel = new System.Windows.Forms.Label { Text = "Variable Name:", Top = 10, Left = 10 };
        variableNameTextBox = new TextBox { Top = 10, Left = 120, Width = 200 };

        System.Windows.Forms.Label variableValueLabel = new System.Windows.Forms.Label { Text = "Variable Value:", Top = 40, Left = 10 };
        variableValueTextBox = new TextBox { Top = 40, Left = 120, Width = 200 };

        addVariableButton = new Button { Text = "Add Variable", Top = 70, Left = 10 };
        addVariableButton.Click += AddVariableButton_Click;

        saveVariablesButton = new Button { Text = "Save Variables", Top = 70, Left = 120 };
        saveVariablesButton.Click += SaveVariablesButton_Click;

        removeVariableButton = new Button { Text = "Remove Variable", Top = 70, Left = 230 };
        removeVariableButton.Click += RemoveVariableButton_Click;

        variablesListBox = new ListBox { Top = 100, Left = 10, Width = 360, Height = 150 };

        this.Controls.Add(variableNameLabel);
        this.Controls.Add(variableNameTextBox);
        this.Controls.Add(variableValueLabel);
        this.Controls.Add(variableValueTextBox);
        this.Controls.Add(addVariableButton);
        this.Controls.Add(saveVariablesButton);
        this.Controls.Add(removeVariableButton);
        this.Controls.Add(variablesListBox);
    }

    private void LoadVariables()
    {
        variablesListBox.Items.Clear();
        foreach (var variable in variables)
        {
            variablesListBox.Items.Add($"{variable.Key}: {variable.Value}");
        }
    }

    private void AddVariableButton_Click(object sender, EventArgs e)
    {
        string name = variableNameTextBox.Text;
        string value = variableValueTextBox.Text;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
        {
            variables[name] = value;
            variablesListBox.Items.Add($"{name}: {value}");
            variableNameTextBox.Clear();
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
            variables.Remove(variableName);
            variablesListBox.Items.Remove(selectedItem);
        }
        else
        {
            MessageBox.Show("Please select a variable to remove");
        }
    }

    private void SaveVariablesButton_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    public Dictionary<string, object> GetVariables()
    {
        return variables;
    }
}

