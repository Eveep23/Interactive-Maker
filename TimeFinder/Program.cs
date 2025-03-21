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
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Core.Initialize();
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private TextBox viewableIdTextBox;
        private TextBox folderPathTextBox;
        private ComboBox initialSegmentComboBox;
        private Button selectFolderButton;
        private Button createSegmentButton;
        private Button moveUpButton;
        private Button moveDownButton;
        private Button deleteSegmentButton;
        private Button saveButton;
        private Button workspaceButton;
        private Button listViewButton;
        private Button managePreconditionsButton;
        private Button manageSegmentGroupsButton;
        private Button zonePositionsButton;
        private Dictionary<string, List<SegmentGroupItem>> segmentGroups = new Dictionary<string, List<SegmentGroupItem>>();
        private ListBox segmentsListBox;
        private Dictionary<string, Segment> segments = new Dictionary<string, Segment>();
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private int viewableId = 10000001;
        private string folderPath = "";
        private long lastPlayedTime = 0;
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Panel buttonBarPanel;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem editMenu;
        private ToolStripMenuItem helpMenu;
        private PictureBox logoPictureBox;

        public MainForm()
        {
            this.Icon = new Icon("icon.ico");
            InitializeComponents();
            LoadSettings();
            if (!string.IsNullOrEmpty(folderPath))
            {
                LoadManifestAndSettingsFromFolder(folderPath);
            }
        }

        private void InitializeComponents()
        {
            this.Text = "Interactive Maker";
            this.Width = 950;
            this.Height = 600;

            // Menu strip
            menuStrip = new MenuStrip();
            menuStrip.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            menuStrip.ForeColor = Color.White;

            fileMenu = new ToolStripMenuItem("File");
            fileMenu.ForeColor = Color.White;
            fileMenu.DropDownOpening += (sender, e) => SetMenuItemColors(fileMenu);

            editMenu = new ToolStripMenuItem("Edit");
            editMenu.ForeColor = Color.White;
            editMenu.DropDownOpening += (sender, e) => SetMenuItemColors(editMenu);

            helpMenu = new ToolStripMenuItem("Help");
            helpMenu.ForeColor = Color.White;
            helpMenu.DropDownOpening += (sender, e) => SetMenuItemColors(helpMenu);

            // File menu items
            var saveJsonsMenuItem = new ToolStripMenuItem("Save JSONs");
            saveJsonsMenuItem.ForeColor = Color.White;
            saveJsonsMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            saveJsonsMenuItem.Click += SaveButton_Click;

            var openFolderMenuItem = new ToolStripMenuItem("Open Folder");
            openFolderMenuItem.ForeColor = Color.White;
            openFolderMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            openFolderMenuItem.Click += SelectFolderButton_Click;

            fileMenu.DropDownItems.Add(saveJsonsMenuItem);
            fileMenu.DropDownItems.Add(openFolderMenuItem);

            // Edit menu items
            var manageVariablesMenuItem = new ToolStripMenuItem("Variables");
            manageVariablesMenuItem.ForeColor = Color.White;
            manageVariablesMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            manageVariablesMenuItem.Click += ManageVariablesButton_Click;

            var managePreconditionsMenuItem = new ToolStripMenuItem("Preconditions");
            managePreconditionsMenuItem.ForeColor = Color.White;
            managePreconditionsMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            managePreconditionsMenuItem.Click += ManagePreconditionsButton_Click;

            var manageSegmentGroupsMenuItem = new ToolStripMenuItem("Segment Groups");
            manageSegmentGroupsMenuItem.ForeColor = Color.White;
            manageSegmentGroupsMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            manageSegmentGroupsMenuItem.Click += ManageSegmentGroupsButton_Click;

            editMenu.DropDownItems.Add(manageVariablesMenuItem);
            editMenu.DropDownItems.Add(managePreconditionsMenuItem);
            editMenu.DropDownItems.Add(manageSegmentGroupsMenuItem);

            // Help menu item
            var helpMenuItem = new ToolStripMenuItem("Discord");
            helpMenuItem.ForeColor = Color.White;
            helpMenuItem.BackColor = ColorTranslator.FromHtml("#2a2a2c");
            helpMenuItem.Click += (sender, e) => System.Diagnostics.Process.Start("https://discord.gg/E4CbrXETsW");

            helpMenu.DropDownItems.Add(helpMenuItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(helpMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Top bar panel
            Panel topBarPanel = new Panel { Top = 20, Left = 0, Width = this.Width, Height = 100, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(topBarPanel);

            // Logo PictureBox
            logoPictureBox = new PictureBox
            {
                Image = System.Drawing.Image.FromFile("InteractiveMakerLogo.png"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Top = 16,
                Left = 10,
                Width = 241,
                Height = 64
            };
            topBarPanel.Controls.Add(logoPictureBox);

            System.Windows.Forms.Label viewableIdLabel = new System.Windows.Forms.Label { Text = "Viewable ID:", Top = 25, Left = 260 };
            viewableIdTextBox = new TextBox { Top = 25, Left = 380, Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            System.Windows.Forms.Label initialSegmentLabel = new System.Windows.Forms.Label { Text = "Initial Segment:", Top = 55, Left = 260 };
            initialSegmentComboBox = new ComboBox { Top = 55, Left = 380, Width = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            createSegmentButton = new Button { Text = "Create New Segment", Top = 10, Left = 600, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            createSegmentButton.Click += CreateSegmentButton_Click;

            saveButton = new Button { Text = "Save JSONs", Top = 10, Left = 760, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            saveButton.Click += SaveButton_Click;

            Button manageVariablesButton = new Button { Text = "Manage Variables", Top = 40, Left = 600, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            manageVariablesButton.Click += ManageVariablesButton_Click;
            topBarPanel.Controls.Add(manageVariablesButton);

            managePreconditionsButton = new Button { Text = "Manage Preconditions", Top = 40, Left = 760, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            managePreconditionsButton.Click += ManagePreconditionsButton_Click;
            topBarPanel.Controls.Add(managePreconditionsButton);

            manageSegmentGroupsButton = new Button { Text = "Manage Segment Groups", Top = 70, Left = 600, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            manageSegmentGroupsButton.Click += ManageSegmentGroupsButton_Click;
            topBarPanel.Controls.Add(manageSegmentGroupsButton);

            zonePositionsButton = new Button { Text = "Zone Positions", Top = 70, Left = 760, Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            zonePositionsButton.Click += ZonePositionsButton_Click;
            topBarPanel.Controls.Add(zonePositionsButton);

            topBarPanel.Controls.Add(viewableIdLabel);
            topBarPanel.Controls.Add(viewableIdTextBox);
            topBarPanel.Controls.Add(initialSegmentLabel);
            topBarPanel.Controls.Add(initialSegmentComboBox);
            topBarPanel.Controls.Add(createSegmentButton);
            topBarPanel.Controls.Add(saveButton);

            // Bar with buttons
            buttonBarPanel = new Panel { Top = 120, Left = 0, Width = this.Width - 350, Height = 40, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(buttonBarPanel);

            workspaceButton = new Button { Text = "Workspace", Top = 5, Left = 10, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            workspaceButton.Click += WorkspaceButton_Click;

            listViewButton = new Button { Text = "List View", Top = 5, Left = 100, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            listViewButton.Click += ListViewButton_Click;

            moveUpButton = new Button { Text = "Move Up", Top = 5, Left = 190, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            moveUpButton.Click += MoveUpButton_Click;

            moveDownButton = new Button { Text = "Move Down", Top = 5, Left = 280, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            moveDownButton.Click += MoveDownButton_Click;

            deleteSegmentButton = new Button { Text = "Delete", Top = 5, Left = 370, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            deleteSegmentButton.Click += DeleteSegmentButton_Click;

            buttonBarPanel.Controls.Add(workspaceButton);
            buttonBarPanel.Controls.Add(listViewButton);
            buttonBarPanel.Controls.Add(moveUpButton);
            buttonBarPanel.Controls.Add(moveDownButton);
            buttonBarPanel.Controls.Add(deleteSegmentButton);

            // Sidebar panel
            sidebarPanel = new Panel { Top = 120, Left = this.Width - 350, Width = 350, Height = this.Height - 130, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right };
            this.Controls.Add(sidebarPanel);

            // Main panel for segment list or grid
            mainPanel = new Panel { Top = 160, Left = 0, Width = this.Width - 350, Height = this.Height - 170, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(mainPanel);

            // Segment list
            segmentsListBox = new ListBox { Dock = DockStyle.Fill };
            segmentsListBox.DoubleClick += SegmentsListBox_DoubleClick;
            mainPanel.Controls.Add(segmentsListBox);
        }

        private void SetMenuItemColors(ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem item in menuItem.DropDownItems)
            {
                item.BackColor = ColorTranslator.FromHtml("#2a2a2c");
                item.ForeColor = Color.White;
            }
        }

        private void ManageVariablesButton_Click(object sender, EventArgs e)
        {
            var variablesForm = new VariablesForm(variables);
            if (variablesForm.ShowDialog() == DialogResult.OK)
            {
                variables = variablesForm.GetVariables();
            }
        }

        private void ManagePreconditionsButton_Click(object sender, EventArgs e)
        {
            var preconditionsForm = new PreconditionsForm(variables);
            if (preconditionsForm.ShowDialog() == DialogResult.OK)
            {
                variables = preconditionsForm.GetPreconditions();
            }
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPath = folderBrowserDialog.SelectedPath;
                    //folderPathTextBox.Text = folderPath;
                    LoadManifestAndSettingsFromFolder(folderPath);
                    SaveSettings();
                }
            }
        }
        private void ManageSegmentGroupsButton_Click(object sender, EventArgs e)
        {
            var preconditionNames = variables.Keys.Where(k => variables[k] is Precondition).ToList();
            var segmentGroupsForm = new SegmentGroupsForm(segmentGroups, segments.Keys.ToList(), preconditionNames);
            if (segmentGroupsForm.ShowDialog() == DialogResult.OK)
            {
                segmentGroups = segmentGroupsForm.GetSegmentGroups();
            }
        }

        private void ZonePositionsButton_Click(object sender, EventArgs e)
        {
            var zoneMakerForm = new ZoneMaker.MainForm();
            zoneMakerForm.Show();
        }

        private void LoadManifestAndSettingsFromFolder(string folderPath)
        {
            string manifestPath = System.IO.Path.Combine(folderPath, "manifest.json");
            string settingsPath = System.IO.Path.Combine(folderPath, "settings.json");
            string infoJsonPath = System.IO.Path.Combine(folderPath, "info.json");

            try
            {
                if (System.IO.File.Exists(manifestPath))
                {
                    string json = System.IO.File.ReadAllText(manifestPath);
                    var manifest = JsonConvert.DeserializeObject<Manifest>(json);
                    viewableId = manifest.viewableId;
                    segments = manifest.segments ?? new Dictionary<string, Segment>();
                    viewableIdTextBox.Text = viewableId.ToString();
                    UpdateSegmentsListBox();
                    UpdateInitialSegmentComboBox();
                    initialSegmentComboBox.SelectedItem = manifest.initialSegment ?? initialSegmentComboBox.Items[0];
                }

                if (System.IO.File.Exists(settingsPath))
                {
                    string json = System.IO.File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<dynamic>(json);
                    lastPlayedTime = settings.LastPlayedTime;
                }

                if (System.IO.File.Exists(infoJsonPath))
                {
                    string json = System.IO.File.ReadAllText(infoJsonPath);
                    var infoJson = JObject.Parse(json);
                    var momentsBySegment = (JObject)infoJson["jsonGraph"]?["videos"]?[viewableId.ToString()]?["interactiveVideoMoments"]?["value"]?["momentsBySegment"];
                    var persistent = (JObject)infoJson["jsonGraph"]?["videos"]?[viewableId.ToString()]?["interactiveVideoMoments"]?["value"]?["stateHistory"]?["persistent"];
                    var preconditions = (JObject)infoJson["jsonGraph"]?["videos"]?[viewableId.ToString()]?["interactiveVideoMoments"]?["value"]?["preconditions"];
                    var segmentGroupsJson = (JObject)infoJson["jsonGraph"]?["videos"]?[viewableId.ToString()]?["interactiveVideoMoments"]?["value"]?["segmentGroups"];

                    if (momentsBySegment != null)
                    {
                        foreach (var segmentKey in segments.Keys.ToList())
                        {
                            if (momentsBySegment.TryGetValue(segmentKey, out JToken segmentData))
                            {
                                var interactionZone = segmentData.FirstOrDefault();
                                if (interactionZone != null)
                                {
                                    segments[segmentKey].ui = new UI
                                    {
                                        interactionZones = new List<long[]>
                                        {
                                            new long[]
                                            {
                                                (long)interactionZone["startMs"],
                                                (long)interactionZone["endMs"]
                                            }
                                        }
                                    };

                                    var choices = interactionZone["choices"];
                                    if (choices != null && choices.Any())
                                    {
                                        segments[segmentKey].choices = choices.ToObject<List<Choice>>();
                                        foreach (var choice in segments[segmentKey].choices)
                                        {
                                            if (choice.impressionData != null && choice.impressionData.ContainsKey("persistent"))
                                            {
                                                var persistentData = choice.impressionData["persistent"] as JObject;
                                                if (persistentData != null)
                                                {
                                                    foreach (var variable in persistentData)
                                                    {
                                                        variables[variable.Key] = variable.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        segments[segmentKey].choices = new List<Choice>();
                                    }
                                }
                                else
                                {
                                    segments[segmentKey].ui = null;
                                    segments[segmentKey].choices = new List<Choice>();
                                }
                            }
                            else
                            {
                                segments[segmentKey].ui = null;
                                segments[segmentKey].choices = new List<Choice>();
                            }
                        }
                    }

                    if (persistent != null)
                    {
                        foreach (var variable in persistent)
                        {
                            variables[variable.Key] = variable.Value;
                        }
                    }

                    if (preconditions != null)
                    {
                        foreach (var precondition in preconditions)
                        {
                            var preconditionArray = precondition.Value as JArray;
                            if (preconditionArray != null && preconditionArray.Count == 3)
                            {
                                var preconditionObj = new Precondition
                                {
                                    Operator = preconditionArray[0].ToString(),
                                    Conditions = preconditionArray[1].ToObject<List<object>>()
                                };
                                preconditionObj.Conditions.Add(preconditionArray[2]);
                                variables[precondition.Key] = preconditionObj;
                            }
                        }
                    }

                    if (segmentGroupsJson != null)
                    {
                        segmentGroups.Clear();
                        foreach (var group in segmentGroupsJson)
                        {
                            var groupName = group.Key;
                            var groupItems = new List<SegmentGroupItem>();
                            foreach (var item in group.Value)
                            {
                                if (item.Type == JTokenType.String)
                                {
                                    groupItems.Add(new SegmentGroupItem { Segment = item.ToString() });
                                }
                                else if (item.Type == JTokenType.Object)
                                {
                                    groupItems.Add(new SegmentGroupItem
                                    {
                                        Segment = item["segment"].ToString(),
                                        Precondition = item["precondition"].ToString()
                                    });
                                }
                            }
                            segmentGroups[groupName] = groupItems;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading manifest and settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateSegmentButton_Click(object sender, EventArgs e)
        {
            var segmentForm = new SegmentForm(segments, null, folderPath, lastPlayedTime, variables.Keys.ToList());
            if (segmentForm.ShowDialog() == DialogResult.OK)
            {
                UpdateSegmentsListBox();
                UpdateInitialSegmentComboBox();
            }
        }

        private void SegmentsListBox_DoubleClick(object sender, EventArgs e)
        {
            if (segmentsListBox.SelectedItem != null)
            {
                string segmentName = segmentsListBox.SelectedItem.ToString();
                var segmentForm = new SegmentForm(segments, segmentName, folderPath, lastPlayedTime, variables.Keys.ToList());
                ShowSegmentFormInSidebar(segmentForm);
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            if (segmentsListBox.SelectedItem != null && segmentsListBox.SelectedIndex > 0)
            {
                int index = segmentsListBox.SelectedIndex;
                var item = segmentsListBox.SelectedItem;
                segmentsListBox.Items.RemoveAt(index);
                segmentsListBox.Items.Insert(index - 1, item);
                segmentsListBox.SelectedIndex = index - 1;
            }
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (segmentsListBox.SelectedItem != null && segmentsListBox.SelectedIndex < segmentsListBox.Items.Count - 1)
            {
                int index = segmentsListBox.SelectedIndex;
                var item = segmentsListBox.SelectedItem;
                segmentsListBox.Items.RemoveAt(index);
                segmentsListBox.Items.Insert(index + 1, item);
                segmentsListBox.SelectedIndex = index + 1;
            }
        }

        private void DeleteSegmentButton_Click(object sender, EventArgs e)
        {
            if (segmentsListBox.SelectedItem != null)
            {
                string segmentName = segmentsListBox.SelectedItem.ToString();
                segments.Remove(segmentName);
                UpdateSegmentsListBox();
                UpdateInitialSegmentComboBox();
            }
        }

        private void UpdateSegmentsListBox()
        {
            segmentsListBox.Items.Clear();
            foreach (var segment in segments.Keys)
            {
                segmentsListBox.Items.Add(segment);
            }
        }

        private void UpdateInitialSegmentComboBox()
        {
            initialSegmentComboBox.Items.Clear();
            foreach (var segment in segments.Keys)
            {
                initialSegmentComboBox.Items.Add(segment);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(viewableIdTextBox.Text, out int id))
            {
                viewableId = id;

                if (!string.IsNullOrEmpty(folderPath))
                {
                    SaveManifest(viewableId, segments, folderPath);
                    MessageBox.Show("Manifest and info JSON saved successfully");
                }
                else
                {
                    MessageBox.Show("Default folder path is not set");
                }
            }
            else
            {
                MessageBox.Show("Invalid Viewable ID");
            }
        }

        private void SaveManifest(int viewableId, Dictionary<string, Segment> segments, string folderPath)
        {
            var manifest = new Manifest
            {
                viewableId = viewableId,
                segments = segments,
                initialSegment = initialSegmentComboBox.SelectedItem?.ToString() ?? string.Empty
            };

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new ExcludeChoicesContractResolver()
            };

            // Save manifest JSON without choices and preconditions
            string manifestJson = JsonConvert.SerializeObject(manifest, settings);
            string manifestPath = System.IO.Path.Combine(folderPath, "manifest.json");
            System.IO.File.WriteAllText(manifestPath, manifestJson);

            // Generate and save the info JSON
            string infoJson = GenerateInfoJson();
            string infoJsonPath = System.IO.Path.Combine(folderPath, "info.json");
            System.IO.File.WriteAllText(infoJsonPath, infoJson);
        }

        public class ExcludeChoicesContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);
                if (type == typeof(Segment))
                {
                    properties = properties.Where(p => p.PropertyName != "choices").ToList();
                }
                return properties;
            }
        }

        private string GenerateInfoJson()
        {
            var infoJson = new JObject
            {
                ["paths"] = new JArray
                {
                    new JArray("videos", viewableId, "interactiveVideoMoments")
                },
                ["jsonGraph"] = new JObject
                {
                    ["videos"] = new JObject
                    {
                        [viewableId.ToString()] = new JObject
                        {
                            ["interactiveVideoMoments"] = new JObject
                            {
                                ["value"] = new JObject
                                {
                                    ["momentsBySegment"] = new JObject(),
                                    ["stateHistory"] = new JObject
                                    {
                                        ["persistent"] = new JObject()
                                    },
                                    ["preconditions"] = new JObject(),
                                    ["segmentGroups"] = new JObject()
                                }
                            }
                        }
                    }
                }
            };

            var momentsBySegment = (JObject)infoJson["jsonGraph"]["videos"][viewableId.ToString()]["interactiveVideoMoments"]["value"]["momentsBySegment"];
            var persistent = (JObject)infoJson["jsonGraph"]["videos"][viewableId.ToString()]["interactiveVideoMoments"]["value"]["stateHistory"]["persistent"];
            var preconditionsJson = (JObject)infoJson["jsonGraph"]["videos"][viewableId.ToString()]["interactiveVideoMoments"]["value"]["preconditions"];
            var segmentGroupsJson = (JObject)infoJson["jsonGraph"]["videos"][viewableId.ToString()]["interactiveVideoMoments"]["value"]["segmentGroups"];

            foreach (var segment in segments)
            {
                var segmentJson = new JArray();

                if (segment.Value.ui != null && segment.Value.ui.interactionZones.Count > 0)
                {
                    var interactionZone = segment.Value.ui.interactionZones[0];
                    var interactionZoneJson = new JObject
                    {
                        ["startMs"] = interactionZone[0],
                        ["endMs"] = interactionZone[1],
                        ["type"] = "scene:cs_template",
                        ["uiDisplayMS"] = interactionZone[0],
                        ["hideTimeoutUiMS"] = interactionZone[1],
                        ["uiHideMS"] = interactionZone[1],
                        ["activationWindow"] = new JArray(interactionZone[0], interactionZone[1])
                    };

                    if (segment.Value.choices != null && segment.Value.choices.Count > 0)
                    {
                        var choicesArray = new JArray();
                        foreach (var choice in segment.Value.choices)
                        {
                            var choiceJson = new JObject
                            {
                                ["segmentId"] = choice.segmentId,
                                ["text"] = choice.text
                            };

                            if (choice.impressionData != null && choice.impressionData.Count > 0)
                            {
                                var impressionDataJson = new JObject();
                                if (choice.impressionData.ContainsKey("data"))
                                {
                                    var data = choice.impressionData["data"] as JObject;
                                    if (data != null && data.ContainsKey("persistent"))
                                    {
                                        impressionDataJson["persistent"] = data["persistent"];
                                    }
                                }
                                else
                                {
                                    impressionDataJson["persistent"] = JObject.FromObject(choice.impressionData);
                                }
                                choiceJson["impressionData"] = new JObject { { "data", impressionDataJson } };
                            }

                            choicesArray.Add(choiceJson);
                        }
                        interactionZoneJson["choices"] = choicesArray;
                    }

                    segmentJson.Add(interactionZoneJson);
                }

                momentsBySegment[segment.Key] = segmentJson;
            }

            // Add variables to the persistent state
            foreach (var variable in variables)
            {
                if (!(variable.Value is Precondition))
                {
                    persistent[variable.Key] = JToken.FromObject(variable.Value);
                }
            }

            // Add preconditions to the info JSON
            foreach (var variable in variables)
            {
                if (variable.Value is Precondition preconditionValue)
                {
                    var preconditionJson = new JArray
                    {
                        preconditionValue.Operator,
                        new JArray(preconditionValue.Conditions.Take(preconditionValue.Conditions.Count - 1)),
                        preconditionValue.Conditions.Last()
                    };
                    preconditionsJson[variable.Key] = preconditionJson;
                }
            }

            // Add segment groups to the info JSON
            foreach (var group in segmentGroups)
            {
                var groupArray = new JArray();
                foreach (var item in group.Value)
                {
                    if (string.IsNullOrEmpty(item.Precondition))
                    {
                        groupArray.Add(item.Segment);
                    }
                    else
                    {
                        groupArray.Add(new JObject
                        {
                            ["segment"] = item.Segment,
                            ["precondition"] = item.Precondition
                        });
                    }
                }
                segmentGroupsJson[group.Key] = groupArray;
            }

            return infoJson.ToString(Formatting.Indented);
        }

        private void SaveSettings()
        {
            var settings = new
            {
                DefaultFolderPath = folderPath,
                LastPlayedTime = lastPlayedTime
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            System.IO.File.WriteAllText("settings.json", json);
        }

        private void LoadSettings()
        {
            if (System.IO.File.Exists("settings.json"))
            {
                string json = System.IO.File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<dynamic>(json);
                folderPath = settings.DefaultFolderPath;
                lastPlayedTime = settings.LastPlayedTime;
                //folderPathTextBox.Text = folderPath;
            }
        }

        private void ShowSegmentFormInSidebar(Form segmentForm)
        {
            sidebarPanel.Controls.Clear();
            segmentForm.TopLevel = false;
            segmentForm.FormBorderStyle = FormBorderStyle.None;
            segmentForm.Dock = DockStyle.Fill;
            sidebarPanel.Controls.Add(segmentForm);
            segmentForm.Show();
        }

        private void WorkspaceButton_Click(object sender, EventArgs e)
        {
            mainPanel.Controls.Clear();
            buttonBarPanel.Controls.Remove(moveUpButton);
            buttonBarPanel.Controls.Remove(moveDownButton);
            buttonBarPanel.Controls.Remove(deleteSegmentButton);
            var segmentsGridForm = new SegmentsGridForm(segments);
            segmentsGridForm.TopLevel = false;
            segmentsGridForm.FormBorderStyle = FormBorderStyle.None;
            segmentsGridForm.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(segmentsGridForm);
            segmentsGridForm.Show();
        }

        private void ListViewButton_Click(object sender, EventArgs e)
        {
            mainPanel.Controls.Clear();
            buttonBarPanel.Controls.Add(moveUpButton);
            buttonBarPanel.Controls.Add(moveDownButton);
            buttonBarPanel.Controls.Add(deleteSegmentButton);
            mainPanel.Controls.Add(segmentsListBox);
        }
    }
}
