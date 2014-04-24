using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    internal class Tiles
    {
        public static void SetTiles(Panel p, bool isFlightMode)
        {
            var tilesFlightMode = new TileInfo[]
            {
                new TileButton("FLIGHT\nINFO", 0, 0, (sender, e) => MainV2.View.ShowScreen("FlightData"),
                    Color.FromArgb(255, 255, 51, 0)),
                new TileData("GROUND SPEED", 0, 1, "km/h"),
                new TileData("ALTITUDE", 0, 2, "m"),
                new TileData("TIME IN THE AIR", 0, 3),
                new TileData("BATTERY REMAINING", 0, 4, "%"),
                new TileData("RADIO SIGNAL", 0, 5, "%"),
                new TileButton("CONNECTION", 0, 6, (sender, args) => MainV2.instance.MenuConnect_Click(null, null)),
                new TileButton("DISARM", 0, 7),
                new TileButton("FLIGHT\nPLANNING", 1, 0, (sender, e) => MainV2.View.ShowScreen("FlightPlanner")),
                new TileData("AIR SPEED", 1, 1, "km/h"),
                new TileData("DISTANCE TO HOME", 1, 2, "km"),
                new TileData("BATTERY VOLTAGE", 1, 3, "V"),
                new TileData("CURRENT", 1, 4, "A"),
                new TileData("GPS SIGNAL", 1, 5, "%"),
                new TileButton("AUTO", 1, 6, null, Color.FromArgb(255, 255, 51, 0)),
                new TileButton("LAND", 1, 7, (sender, args) => FlightPlanner.instance.landToolStripMenuItem_Click(null, null)),
                new TileButton("RESTART", 2, 6),
                new TileButton("RETURN", 2, 7),
                new TileData("WIND SPED", 9, 0, "m/s"),
            };

            // todo copy paste code ;/
            var tilesFlightPlanning = new TileInfo[]
            {
                new TileButton("FLIGHT\nINFO", 0, 0, (sender, e) => MainV2.View.ShowScreen("FlightData")),
                new TileButton("POLYGON\nMODE", 0, 1),
                new TileButton("ADD START\nPOINT", 0, 2, (sender, args) => FlightPlanner.instance.takeoffToolStripMenuItem_Click(null, null)),
                new TileButton("CLEAR", 0, 3, (sender, args) =>
                {
                    FlightPlanner.instance.clearMissionToolStripMenuItem_Click(null, null);
                    FlightPlanner.instance.clearPolygonToolStripMenuItem_Click(null, null);
                }), 
                new TileData("DISTANCE", 0, 4, "km"),
                new TileData("RADIO SIGNAL", 0, 5, "km2"),
                new TileButton("CONNECTION", 0, 6, (sender, args) => MainV2.instance.MenuConnect_Click(null, null)),
                new TileButton("DISARM", 0, 7),
                new TileButton("FLIGHT\nPLANNING", 1, 0, (sender, e) => MainV2.View.ShowScreen("FlightPlanner"),
                    Color.FromArgb(255, 255, 51, 0)),
                    
                new TileButton("WAYPOINT\nMODE", 1, 1, (sender, e) =>
                {
                    var Host = new Plugin.PluginHost();
                    ToolStripItemCollection col = Host.FPMenuMap.Items;
                    int index = col.Count;
                    foreach (var toolStripItem in col.Cast<ToolStripItem>().Where(item => item.Text.Equals("Auto WP")).OfType<ToolStripMenuItem>().SelectMany(toolStripMenuItem => toolStripMenuItem.DropDownItems.Cast<object>()
                        .OfType<ToolStripItem>()
                        .Where(toolStripItem => toolStripItem.Text.Equals("Survey (Grid)"))))
                    {
                        toolStripItem.PerformClick();
                    }
                }),
                new TileButton("ADD LANDING POINT", 1, 2, (sender, args) => FlightPlanner.instance.landToolStripMenuItem_Click(null, null)),
                new TileData("OBSERVATION HEAD", 1, 3),
                new TileData("ANGLE", 1, 4, "degrees"),
                new TileData("ALTITUDE", 1, 5, "m"),
                new TileButton("AUTO", 1, 6, null, Color.FromArgb(255, 255, 51, 0)),
                new TileButton("LAND", 1, 7, (sender, args) => FlightPlanner.instance.landToolStripMenuItem_Click(null, null)),
                new TileButton("RESTART", 2, 6),
                new TileButton("RETURN", 2, 7),
                new TileData("WIND SPED", 9, 0, "m/s"),
            };

            var tilesArray = (isFlightMode) ? tilesFlightMode : tilesFlightPlanning;

            foreach (var tile in tilesArray)
            {
                //TODO: transparent
                var panel = new Panel
                {
                    Size = new Size(158, 64),
                    Location = new Point(tile.Column*160, tile.Row*66),
                    BackColor = Color.FromArgb(220, 0, 0, 0),
                    Parent = p
                };

                panel.Controls.Add(tile.Label);

                p.Controls.Add(panel);
                panel.BringToFront();
            }

        }
    }

    abstract class TileInfo
    {
        protected readonly string text;

        public int Row { get; private set; }
        public int Column { get; private set; }

        protected TileInfo(string text, int row, int column)
        {
            this.text = text;
            Row = row;
            Column = column;
        }

        public abstract Control Label { get; }
    }

    class TileData : TileInfo
    {
        private readonly string unit;
        public TileData(string text, int row, int column, string unit = "")
            : base(text, row, column)
        {
            this.unit = unit;
        }

        public override Control Label
        {
            get
            {
                var panel = new Panel();
                panel.Size=new Size(158, 64);
               // panel.Dock = DockStyle.Fill;
                ;
                var headLabel = new Label()
                {
                    Text = text,
                    ForeColor = Color.FromArgb(255, 41, 171, 226),
                    Font = new Font("Century Gothic", 10, FontStyle.Italic),
                    Top = 10,
                    Left = 10
                };
                var unitLabel = new Label()
                {
                    Text = unit,
                    ForeColor = Color.White,
                    Font = new Font("Century Gothic", 12),
                    TextAlign = ContentAlignment.BottomRight,
                };
                unitLabel.Top = 64 - unitLabel.Height - 12;
                unitLabel.Left = 158 - unitLabel.Width - 10;

                var valueLabel = new Label()
                {
                    ForeColor = Color.White,
                    Font = new Font("Century Gothic", 18),
                    Left = 10,
                    Text = "0",
                    Height = 25,
                };
                valueLabel.Top = 64 - valueLabel.Height - 12;
                panel.Controls.Add(unitLabel);
                panel.Controls.Add(valueLabel);valueLabel.BringToFront();
                panel.Controls.Add(headLabel);
                panel.Dock = DockStyle.Fill;
                return panel;
            }
        }
    }

    class TileButton : TileInfo
    {
        private readonly Color color;

        public TileButton(string text, int row, int column, EventHandler handler = null, Color? color = null)
            : base(text, row, column)
        {
            this.color = color == null ? Color.White : color.GetValueOrDefault();
            ClickMethod += handler;
        }

        public EventHandler ClickMethod;

        public override Control Label
        {
            get
            {
                var label = new Label
                {
                    Text = text,
                    ForeColor = color,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Century Gothic", 14)
                };
                label.Click += ClickMethod;
                return label;
            }
        }
    }

}
