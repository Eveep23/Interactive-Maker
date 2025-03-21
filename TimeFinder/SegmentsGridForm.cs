using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace TimeFinder
{
    public class SegmentsGridForm : Form
    {
        private Dictionary<string, Segment> segments;
        private Dictionary<string, Rectangle> segmentRectangles;
        private Point panOffset;
        private bool isPanning;
        private Point lastMousePosition;
        private bool isDragging;
        private string draggingSegment;
        private Button saveButton;
        private Button loadButton;
        private float zoomLevel = 1.0f; // Add zoom level field

        public SegmentsGridForm(Dictionary<string, Segment> segments)
        {
            this.segments = segments;
            this.segmentRectangles = new Dictionary<string, Rectangle>();
            this.panOffset = new Point(0, 0);
            this.Text = "Segments Grid";
            this.Width = 800;
            this.Height = 600;
            this.BackColor = Color.White;
            this.DoubleBuffered = true; // Enable double buffering
            this.Paint += SegmentsGridForm_Paint;
            this.MouseDown += SegmentsGridForm_MouseDown;
            this.MouseMove += SegmentsGridForm_MouseMove;
            this.MouseUp += SegmentsGridForm_MouseUp;
            this.MouseWheel += SegmentsGridForm_MouseWheel;
            this.MouseDoubleClick += SegmentsGridForm_MouseDoubleClick;

            // Add save and load buttons
            saveButton = new Button { Text = "Save", Top = 10, Left = 10 };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            loadButton = new Button { Text = "Load", Top = 10, Left = 100 };
            loadButton.Click += LoadButton_Click;
            this.Controls.Add(loadButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var segmentPositions = new Dictionary<string, Rectangle>();
            foreach (var segment in segmentRectangles)
            {
                segmentPositions[segment.Key] = segment.Value;
            }

            string json = JsonConvert.SerializeObject(segmentPositions, Formatting.Indented);
            File.WriteAllText("segment_positions.json", json);
        }
        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (File.Exists("segment_positions.json"))
            {
                string json = File.ReadAllText("segment_positions.json");
                var segmentPositions = JsonConvert.DeserializeObject<Dictionary<string, Rectangle>>(json);

                foreach (var segment in segmentPositions)
                {
                    if (segmentRectangles.ContainsKey(segment.Key))
                    {
                        segmentRectangles[segment.Key] = segment.Value;
                    }
                }

                Invalidate();
            }
            else
            {
                MessageBox.Show("No saved segment positions found.");
            }
        }

        private void SegmentsGridForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(zoomLevel, zoomLevel); // Apply zoom transformation

            // Draw grid background
            DrawGrid(g);

            int x = 50;
            int y = 50;
            int width = 100;
            int height = 40; // Make the boxes a little shorter
            int margin = 20;

            Pen lightBluePen = new Pen(Color.LightBlue, 2); // Make the blue a little lighter

            // Draw segments as rectangles with rounded corners
            foreach (var segment in segments)
            {
                if (!segmentRectangles.ContainsKey(segment.Key))
                {
                    Rectangle rect = new Rectangle(x, y, width, height);
                    segmentRectangles[segment.Key] = rect;
                    x += width + margin;
                }

                Rectangle segmentRect = segmentRectangles[segment.Key];

                // Fill the inside of the segment box with a white background
                using (Brush whiteBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(whiteBrush, segmentRect);
                }

                DrawRoundedRectangle(g, lightBluePen, segmentRect, 10); // Round the corners of the boxes

                // Center the text in the middle of the rectangle
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(segment.Key, this.Font, Brushes.Black, segmentRect, stringFormat);
            }

            // Draw lines connecting segments to their default next segments
            foreach (var segment in segments)
            {
                if (!string.IsNullOrEmpty(segment.Value.defaultNext) && segments.ContainsKey(segment.Value.defaultNext))
                {
                    DrawConnectionLine(g, segment.Key, segment.Value.defaultNext);
                }

                // Draw lines connecting segments to their choices
                if (segment.Value.choices != null)
                {
                    foreach (var choice in segment.Value.choices)
                    {
                        if (!string.IsNullOrEmpty(choice.segmentId) && segments.ContainsKey(choice.segmentId))
                        {
                            DrawConnectionLine(g, segment.Key, choice.segmentId);
                        }
                    }
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            int gridSize = 50; // Increase the size of the grid squares
            Pen gridPen = new Pen(Color.LightGray);

            // Calculate the starting points based on the pan offset and zoom level
            int startX = (int)(-panOffset.X / zoomLevel) / gridSize * gridSize;
            int startY = (int)(-panOffset.Y / zoomLevel) / gridSize * gridSize;

            // Calculate the ending points based on the form size and zoom level
            int endX = (int)((this.Width - panOffset.X) / zoomLevel);
            int endY = (int)((this.Height - panOffset.Y) / zoomLevel);

            for (int x = startX; x < endX; x += gridSize)
            {
                g.DrawLine(gridPen, x, startY, x, endY);
            }

            for (int y = startY; y < endY; y += gridSize)
            {
                g.DrawLine(gridPen, startX, y, endX, y);
            }
        }

        private void DrawConnectionLine(Graphics g, string fromSegmentKey, string toSegmentKey)
        {
            Rectangle fromRect = segmentRectangles[fromSegmentKey];
            Rectangle toRect = segmentRectangles[toSegmentKey];
            Point fromPoint = new Point(fromRect.Right, fromRect.Top + fromRect.Height / 2);
            Point toPoint = new Point(toRect.Left, toRect.Top + toRect.Height / 2);

            // Calculate control points for Bezier curve
            Point controlPoint1 = new Point((fromPoint.X + toPoint.X) / 2, fromPoint.Y);
            Point controlPoint2 = new Point((fromPoint.X + toPoint.X) / 2, toPoint.Y);

            // Draw Bezier curve
            g.DrawBezier(Pens.Black, fromPoint, controlPoint1, controlPoint2, toPoint);
        }

        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.DrawPath(pen, path);
            }
        }

        private void SegmentsGridForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point adjustedLocation = new Point((int)((e.Location.X - panOffset.X) / zoomLevel), (int)((e.Location.Y - panOffset.Y) / zoomLevel));
                foreach (var segment in segmentRectangles)
                {
                    if (segment.Value.Contains(adjustedLocation))
                    {
                        isDragging = true;
                        draggingSegment = segment.Key;
                        lastMousePosition = adjustedLocation;
                        return;
                    }
                }

                isPanning = true;
                lastMousePosition = e.Location;
            }
        }

        private void SegmentsGridForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panOffset.X += e.X - lastMousePosition.X;
                panOffset.Y += e.Y - lastMousePosition.Y;
                lastMousePosition = e.Location;
                Invalidate();
            }
            else if (isDragging && draggingSegment != null)
            {
                Point adjustedLocation = new Point((int)((e.Location.X - panOffset.X) / zoomLevel), (int)((e.Location.Y - panOffset.Y) / zoomLevel));
                var rect = segmentRectangles[draggingSegment];

                // Calculate the nearest grid point
                int gridSize = 50;
                int newX = (adjustedLocation.X / gridSize) * gridSize;
                int newY = (adjustedLocation.Y / gridSize) * gridSize + gridSize / 2 - rect.Height / 2;

                rect.X = newX;
                rect.Y = newY;
                segmentRectangles[draggingSegment] = rect;
                lastMousePosition = adjustedLocation;
                Invalidate();
            }
        }

        private void SegmentsGridForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = false;
                isDragging = false;
                draggingSegment = null;
            }
        }

        private void SegmentsGridForm_MouseWheel(object sender, MouseEventArgs e)
        {
            // Calculate the position of the mouse relative to the content before zooming
            Point mouseBeforeZoom = new Point((int)((e.Location.X - panOffset.X) / zoomLevel), (int)((e.Location.Y - panOffset.Y) / zoomLevel));

            if (e.Delta > 0)
            {
                zoomLevel += 0.1f; // Zoom in
            }
            else
            {
                zoomLevel -= 0.1f; // Zoom out
                if (zoomLevel < 0.1f) zoomLevel = 0.1f; // Prevent zooming out too much
            }

            // Calculate the position of the mouse relative to the content after zooming
            Point mouseAfterZoom = new Point((int)((e.Location.X - panOffset.X) / zoomLevel), (int)((e.Location.Y - panOffset.Y) / zoomLevel));

            // Adjust the panOffset to keep the content under the cursor in the same position
            panOffset.X += (int)((mouseAfterZoom.X - mouseBeforeZoom.X) * zoomLevel);
            panOffset.Y += (int)((mouseAfterZoom.Y - mouseBeforeZoom.Y) * zoomLevel);

            Invalidate();
        }

        private void SegmentsGridForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point adjustedLocation = new Point((int)((e.Location.X - panOffset.X) / zoomLevel), (int)((e.Location.Y - panOffset.Y) / zoomLevel));
            foreach (var segment in segmentRectangles)
            {
                if (segment.Value.Contains(adjustedLocation))
                {
                    string segmentName = segment.Key;
                    var segmentForm = new SegmentForm(segments, segmentName, "", 0, new List<string>());
                    segmentForm.ShowDialog();
                    return;
                }
            }
        }
    }
}
