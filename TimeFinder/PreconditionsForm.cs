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
using Newtonsoft.Json.Serialization;
using System.Drawing;

namespace TimeFinder
{
    public class PreconditionsForm : Form
    {
        private TextBox preconditionNameTextBox;
        private ComboBox variableNameComboBox;
        private TextBox variableValueTextBox;
        private Button addPreconditionButton;
        private Button savePreconditionsButton;
        private Button removePreconditionButton;
        private ListBox preconditionsListBox;
        private Dictionary<string, Precondition> preconditions;
        private List<string> variableNames;

        public PreconditionsForm(Dictionary<string, object> variables)
        {
            this.Icon = new Icon("icon.ico");
            this.preconditions = variables.Where(v => v.Value is Precondition)
                                          .ToDictionary(v => v.Key, v => (Precondition)v.Value);
            this.variableNames = variables.Keys.ToList();
            InitializeComponents();
            LoadPreconditions();
        }

        private void InitializeComponents()
        {
            this.Text = "Manage Preconditions";
            this.Width = 400;
            this.Height = 300;

            System.Windows.Forms.Label preconditionNameLabel = new System.Windows.Forms.Label { Text = "Precondition Name:", Top = 10, Left = 10 };
            preconditionNameTextBox = new TextBox { Top = 10, Left = 150, Width = 200 };

            System.Windows.Forms.Label variableNameLabel = new System.Windows.Forms.Label { Text = "Variable Name:", Top = 40, Left = 10 };
            variableNameComboBox = new ComboBox { Top = 40, Left = 150, Width = 200 };
            variableNameComboBox.Items.AddRange(variableNames.ToArray());

            System.Windows.Forms.Label variableValueLabel = new System.Windows.Forms.Label { Text = "Variable Value:", Top = 70, Left = 10 };
            variableValueTextBox = new TextBox { Top = 70, Left = 150, Width = 200 };

            addPreconditionButton = new Button { Text = "Add Precondition", Top = 100, Left = 10 };
            addPreconditionButton.Click += AddPreconditionButton_Click;

            savePreconditionsButton = new Button { Text = "Save Preconditions", Top = 100, Left = 120 };
            savePreconditionsButton.Click += SavePreconditionsButton_Click;

            removePreconditionButton = new Button { Text = "Remove Precondition", Top = 100, Left = 230 };
            removePreconditionButton.Click += RemovePreconditionButton_Click;

            preconditionsListBox = new ListBox { Top = 130, Left = 10, Width = 360, Height = 150 };

            this.Controls.Add(preconditionNameLabel);
            this.Controls.Add(preconditionNameTextBox);
            this.Controls.Add(variableNameLabel);
            this.Controls.Add(variableNameComboBox);
            this.Controls.Add(variableValueLabel);
            this.Controls.Add(variableValueTextBox);
            this.Controls.Add(addPreconditionButton);
            this.Controls.Add(savePreconditionsButton);
            this.Controls.Add(removePreconditionButton);
            this.Controls.Add(preconditionsListBox);
        }

        private void LoadPreconditions()
        {
            preconditionsListBox.Items.Clear();
            foreach (var precondition in preconditions)
            {
                preconditionsListBox.Items.Add($"{precondition.Key}: {precondition.Value.Operator} {string.Join(", ", precondition.Value.Conditions)}, {precondition.Value.Conditions.Last()}");
            }
        }

        private void AddPreconditionButton_Click(object sender, EventArgs e)
        {
            string preconditionName = preconditionNameTextBox.Text;
            string variableName = variableNameComboBox.SelectedItem?.ToString();
            string variableValue = variableValueTextBox.Text;

            if (!string.IsNullOrEmpty(preconditionName) && !string.IsNullOrEmpty(variableName) && !string.IsNullOrEmpty(variableValue))
            {
                var precondition = new Precondition
                {
                    Operator = "eql",
                    Conditions = new List<object> { "persistentState", variableName, variableValue }
                };
                preconditions[preconditionName] = precondition;
                preconditionsListBox.Items.Add($"{preconditionName}: {precondition.Operator} {string.Join(", ", precondition.Conditions.Take(precondition.Conditions.Count - 1))}, {variableValue}");
                preconditionNameTextBox.Clear();
                variableNameComboBox.SelectedIndex = -1;
                variableValueTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Precondition name, variable name, and variable value cannot be empty");
            }
        }

        private void RemovePreconditionButton_Click(object sender, EventArgs e)
        {
            if (preconditionsListBox.SelectedItem != null)
            {
                string selectedItem = preconditionsListBox.SelectedItem.ToString();
                string preconditionName = selectedItem.Split(':')[0].Trim();
                preconditions.Remove(preconditionName);
                preconditionsListBox.Items.Remove(selectedItem);
            }
            else
            {
                MessageBox.Show("Please select a precondition to remove");
            }
        }

        private void SavePreconditionsButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public Dictionary<string, object> GetPreconditions()
        {
            return preconditions.ToDictionary(p => p.Key, p => (object)p.Value);
        }
    }
}
