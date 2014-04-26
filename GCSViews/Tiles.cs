using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Amazon.SimpleEmail.Model;

namespace MissionPlanner.GCSViews
{
    internal class Tiles
    {
        public static void SetTiles(Panel p, bool isFlightMode)
        {
            var tilesFlightMode = new List<TileInfo>(new TileInfo[]
            {
                new TileButton("FLIGHT\nINFO", 0, 0, (sender, e) => MainV2.View.ShowScreen("FlightData"),
                    Color.FromArgb(255, 255, 51, 0)),
                new TileData("GROUND SPEED", 0, 1, "km/h"),
                new TileData("ALTITUDE", 0, 2, "m"),
                new TileData("TIME IN THE AIR", 0, 3),
                new TileData("BATTERY REMAINING", 0, 4, "%"),
                new TileData("RADIO SIGNAL", 0, 5, "%"),
                new TileButton("DISARM", 0, 7),
                new TileButton("FLIGHT\nPLANNING", 1, 0, (sender, e) => MainV2.View.ShowScreen("FlightPlanner")),
                new TileData("AIR SPEED", 1, 1, "km/h"),
                new TileData("DISTANCE TO HOME", 1, 2, "km"),
                new TileData("BATTERY VOLTAGE", 1, 3, "V"),
                new TileData("CURRENT", 1, 4, "A"),
                new TileData("GPS SIGNAL", 1, 5, "%"),
            });

            
            var defaultHead = new TileButton("DEFAULT", 2, 3, (sender, args) => {});
            var cam1Head = new TileButton("CAMERA 1", 3, 3);
            
            var cam2Head = new TileButton("CAMERA 2", 4, 3);
            var obsHeadBtn = new TileData("OBSERVATION HEAD", 1, 3, string.Empty);
            obsHeadBtn.ClickMethod += (sender, args) =>
            {
                defaultHead.Label.Visible = cam1Head.Label.Visible = cam2Head.Label.Visible = true;
                var label = sender as Label;
                if (label != null) obsHeadBtn.Label.Text = label.Text;
            };

            EventHandler fnc = (sender, args) =>
            {
                defaultHead.Label.Visible =
                    cam1Head.Label.Visible = cam2Head.Label.Visible = false;
            };
            fnc(null, null);
            defaultHead.ClickMethod += fnc;
            cam1Head.ClickMethod += fnc;
            cam2Head.ClickMethod += fnc;

            var angleBtnUp = new TileButton("+25", 3, 4);
            var angleBtnDown = new TileButton("-25", 4, 4);
            var angleBtnOk = new TileButton("OK", 5, 4);
            var altBtnUp = new TileButton("+25", 3, 4);
            var altBtnDown = new TileButton("-25", 4, 4);
            var altBtnOk = new TileButton("OK", 5, 4);
            
            const string polygonmodestring = "POLYGON\nMODE";
            var tilesFlightPlanning = new List<TileInfo>(new TileInfo[]
            {
                obsHeadBtn,/*, defaultHead, cam1Head, cam2Head, angleBtnUp, angleBtnDown, angleBtnOk, altBtnUp, altBtnDown, altBtnOk,*/
                new TileButton("FLIGHT\nINFO", 0, 0, (sender, e) => MainV2.View.ShowScreen("FlightData")),
                new TileButton(polygonmodestring, 0, 1, (sender, e) =>
                {
                    var s = sender as Label;
                    // todo YEAH HACKING EVERYWHERE!
                    if (s.Text == polygonmodestring)
                    {
                        s.Text = "WAYPOINT\nMODE";
                        FlightPlanner.instance.PolygonGridMode = false;
                    }
                    else
                    {
                        s.Text = polygonmodestring;
                        FlightPlanner.instance.PolygonGridMode = true;
                    }
                }),
                new TileButton("ADD START\nPOINT", 0, 2, (sender, args) => FlightPlanner.instance.takeoffToolStripMenuItem_Click(null, null)),
                new TileButton("CLEAR", 0, 3, (sender, args) =>
                {
                    FlightPlanner.instance.clearMissionToolStripMenuItem_Click(null, null);
                    FlightPlanner.instance.clearPolygonToolStripMenuItem_Click(null, null);
                }), 
                new TileData("DISTANCE", 0, 4, "km"),
                new TileData("RADIO SIGNAL", 0, 5, "km2"),                
                new TileButton("FLIGHT\nPLANNING", 1, 0, (sender, e) => MainV2.View.ShowScreen("FlightPlanner"),
                    Color.FromArgb(255, 255, 51, 0)),
                    
                new TileButton("PATH\nGENERATION", 1, 1, (sender, e) =>
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
               
                new TileData("ANGLE", 1, 4, "degrees"),
                new TileData("ALTITUDE", 1, 5, "m"),        
            });

            var tilesArray = (isFlightMode) ? tilesFlightMode : tilesFlightPlanning;

            tilesArray.Add(new TileButton("CONNECTION", 0, 6, (sender, args) => MainV2.instance.MenuConnect_Click(null, null)));
            tilesArray.Add(new TileButton("AUTO", 1, 6, (sender, e) =>
            {
                try
                {
                    MainV2.comPort.setMode("Auto");
                }
                catch
                {
                    CustomMessageBox.Show("The Command failed to execute", "Error");
                }
            }, Color.FromArgb(255, 255, 51, 0)));
            tilesArray.Add(new TileButton("RESTART", 2, 6, (sender, args) =>
            {
                try
                {
                    MainV2.comPort.setWPCurrent(0);
                }
                catch { CustomMessageBox.Show("The command failed to execute", "Error"); }
            }));
            tilesArray.Add(new TileButton("RETURN", 2, 7, (sender, args) =>
            {
                try
                {
                    MainV2.comPort.setMode("RTL");
                }
                catch { CustomMessageBox.Show("The Command failed to execute", "Error"); }
            }));
            tilesArray.Add(new TileButton("LAND", 1, 7)); // todo not implemented
               // (sender, args) => FlightPlanner.instance.landToolStripMenuItem_Click(null, null)));     
            tilesArray.Add(new TileButton("ARM/DISARM", 0, 7, (sender, args) => FlightData.instance.BUT_ARM_Click(sender, args)));
            tilesArray.Add(new TileData("WIND SPED", 9, 0, "m/s"));

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

        public string Text
        {
            get { return text; }
        }

        public abstract Control Label { get; }
    }

    class TileData : TileInfo
    {
        private readonly string unit;
        public TileData(string text, int row, int column, string unit = "", EventHandler handler = null)
            : base(text, row, column)
        {
            this.unit = unit;
            ClickMethod = handler;
        }

        public EventHandler ClickMethod;

        public override Control Label
        {
            get
            {
                var panel = new Panel {Size = new Size(158, 64)};
                // panel.Dock = DockStyle.Fill;
                ;
                var headLabel = new Label()
                {
                    Text = text,
                    ForeColor = Color.FromArgb(255, 41, 171, 226),
                    Font = new Font("Century Gothic", 10, FontStyle.Italic),
                    Top = 10,
                    Left = 10,
                    Width =  160
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
                panel.Click += ClickMethod;
                foreach (var label in panel.Controls.OfType<Label>())
                {
                    label.Click += ClickMethod;
                }
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
