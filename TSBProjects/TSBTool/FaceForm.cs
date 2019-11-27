using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TSBTool
{
	/// <summary>
	/// Summary description for FaceForm.
	/// </summary>
	public class FaceForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private static Image[] m_Faces = null;
        private Button mCancelButton;
		private int m_ImageIndex = -1;

		public FaceForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			GetFaces();
		}

		/// <summary>
		/// The index of the selected image.
		/// </summary>
		public int ImageIndex
		{
			get
			{
				return m_ImageIndex;
			}
		}

		private void GetFaces()
		{
			if( m_Faces == null )
			{
				m_Faces = new Image[0xD5];
				string file;

				for( int i = 0; i < 0x53 ; i ++)
				{
					file = "TSBTool.FACES."+string.Format("{0:x2}.BMP",i).ToUpper();
					m_Faces[i] = StaticUtils.GetImage(file);
				}

				for(int i = 0x80; i < 0xD5; i++)
				{
					file = "TSBTool.FACES."+string.Format("{0:x2}.BMP",i).ToUpper();
					m_Faces[i] = StaticUtils.GetImage(file);
				}
			}
		}

		private int GetImageIndex(int x, int y)
		{
			int ret = -1;
			int col = x /(m_Space + m_Faces[0].Width);
			int row = y /(m_Space + m_Faces[0].Height);
			if( row < 6 )
			{
				ret = row * m_ImagesPerRow + col;
			}
			else
			{
				y -= m_RaceSpacer;
				col = x /(m_Space + m_Faces[0].Width);
				row = y  /(m_Space + m_Faces[0].Height);
				row -= 6;
				ret = row * m_ImagesPerRow + col;
				ret += 0x80;
			}

			if( ret > 0xD4 || (ret > 0x52 && ret < 0x80) )
				ret = -1;
			return ret;
		}
		// used for image placement.
		private int m_StartX = 5;
		private int m_StartY = 5;
		private int m_Space  = 5;
		private int m_RaceSpacer = 20;
		private int m_WhiteStart = 0x00;
		private int m_WhiteEnd   = 0x54;
		private int m_BlackStart = 0x80;
		private int m_BlackEnd   = 0xD5;
		private int m_ImagesPerRow = 14;

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);
			// 0x00 - 0x53, 21 per line
			// start at 5,5
			int currentX = m_StartX;
			int currentY = m_StartY;
			int imgHeight, imgWidth;
			imgHeight = imgWidth = 0;
			Image current;

			for( int i = m_WhiteStart; i < m_WhiteEnd; i++)
			{
				current   = m_Faces[i];
				if( current != null )
				{
					imgHeight = current.Height;
					imgWidth  = current.Width;
					e.Graphics.DrawImage(current, currentX, currentY, imgHeight, imgWidth);
					currentX += (imgWidth + m_Space);
					if( (i+1) % m_ImagesPerRow == 0 )
					{
						currentY += (imgHeight + m_Space);
						currentX  = m_Space;
					}
				}
				else
				{
					//MessageBox.Show(string.Format("Bad index {0:x2} ",i));
				}
			}
			currentX = m_StartX;
			currentY += (imgHeight+ m_RaceSpacer);
			int k = 1;
			//0x80 - 0xD4
			for( int i = m_BlackStart; i < m_BlackEnd; i++)
			{
				current   = m_Faces[i];
				if( current != null )
				{
					imgHeight = current.Height;
					imgWidth  = current.Width;
					e.Graphics.DrawImage(current, currentX, currentY, imgHeight, imgWidth);
					currentX += (imgWidth + m_Space);
					if( k++ % m_ImagesPerRow == 0 )
					{
						currentY += (imgHeight + m_Space);
						currentX  = m_Space;
					}
				}
				else
				{
					//MessageBox.Show(string.Format("Bad index {0:x2} ",i));
				}
			}
	}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FaceForm));
            this.mCancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(447, 514);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 0;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // FaceForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(520, 498);
            this.Controls.Add(this.mCancelButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(536, 536);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(536, 536);
            this.Name = "FaceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pick your Face.";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FaceForm_MouseDown);
            this.ResumeLayout(false);

		}
		#endregion

		private void FaceForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int imageIndex = GetImageIndex(e.X, e.Y);
			if( imageIndex > -1 )
			{
				m_ImageIndex = imageIndex;
				//this.Close();
			}
			this.Close();
		}


	}
}
