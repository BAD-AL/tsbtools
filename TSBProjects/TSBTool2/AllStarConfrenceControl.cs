using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace TSBTool2
{
    public partial class AllStarConfrenceControl : UserControl
    {
        public AllStarConfrenceControl()
        {
            InitializeComponent();
        }

        public void ReInitialize()
        {
            foreach (Control ctrl in Controls)
            {
                AllstarPlayerControl asp = ctrl as AllstarPlayerControl;
                {
                    if (asp != null)
                        asp.ReInitialize();
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AddPlayerControls();
        }

        public Conference Conference { get; set; }

        private Control[] mPlayerControls = new Control[TSB2Tool.positionNames.Count];

        private void AddPlayerControls()
        {
            TSBPlayer player;
            for (int i = 0; i < TSB2Tool.positionNames.Count; i++)
            {
                player = (TSBPlayer)i;
                AllstarPlayerControl aspc = new AllstarPlayerControl();
                aspc.Top = i * aspc.Height;
                aspc.Width = Width - 3;
                aspc.PlayerPosition = player;
                aspc.Name = player.ToString();
                aspc.Conference = this.Conference;
                aspc.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
                aspc.Data = this.Data;
                mPlayerControls[i] = aspc;
            }
            
            SuspendLayout();
            Controls.AddRange(mPlayerControls);
            ResumeLayout();
        }

        /// <summary>
        /// The Text / player data from main gui
        /// </summary>
        public String Data { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(2000);
            builder.Append("# ");
            builder.Append(Conference.ToString());
            builder.Append(" ProBowl players\r\n");
            for (int i = 0; i < mPlayerControls.Length; i++)
            {
                if (mPlayerControls[i] != null)
                {
                    builder.Append(mPlayerControls[i].ToString());
                    builder.Append("\r\n");
                }
            }
            return builder.ToString();
        }
    }
}
