using System;
using System.Drawing;

namespace PlayProto
{
	/// <summary>
	/// Summary description for ImageGuy.
	/// </summary>
	public class ImageGuy
	{
		private Image mImage = null;

		/// <summary>
		/// The Image associated with this ImageGuy.
		/// </summary>
		public Image TheImage
		{
			get{ return mImage; }
		}

		private string mFileName = null;

		/// <summary>
		/// The Filename the Image comes from.
		/// </summary>
		public string FileName 
		{
			get{ return mFileName; }
			set
			{
				string mPrev = mFileName;
				try
				{
					mFileName = value;
					//mImage = Image.FromFile(mFileName);
                    mImage = GetImage( "PlayProto.Tiles." +mFileName);
				}
				catch
				{
					mFileName = mPrev;
				}
			}
		}

        private Image GetImage(string file)
        {
            Image ret = null;
            try
            {
                System.IO.Stream s = this.GetType().Assembly.GetManifestResourceStream(file);
                if (s != null)
                    ret = Image.FromStream(s);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            return ret;
        }

		/// <summary>
		/// if FileName = "jarjar.bmp", this property will return "jarjar".
		/// </summary>
		public string FileNameNoExt
		{
			get
			{
				string ret = null;
				if( mFileName != null )
				{
					int pos1 = mFileName.LastIndexOf("\\");
					int pos = mFileName.LastIndexOf(".");
					ret = mFileName.Substring( pos1+1, (pos-pos1)-1);
				}
				return ret;
			}
		}

		/// <summary>
		/// returns a value like 0x01, 0x34, ...
		/// </summary>
		public byte TileNumber
		{
			get
			{
				byte ret = 0x00;
				try
				{
					ret = Byte.Parse(FileNameNoExt,System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.Assert(false, e.Message );
				}
				return ret;
			}
		}

		/// <summary>
		/// Constructs an ImageGuy.
		/// </summary>
		public ImageGuy()
		{
		}

		/// <summary>
		/// Constructs an ImageGuy with 'fileName' as the image.
		/// </summary>
		/// <param name="fileName"></param>
		public ImageGuy(string fileName)
		{
			FileName = fileName;
		}
	}
}
