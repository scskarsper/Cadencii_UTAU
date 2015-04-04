using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using cadencii.dsp.generator.STP;

namespace AudioUtils
{
	/// <summary>
	/// Summary description for WaveControl.
	/// </summary>
	public class WaveControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// This is the WaveFile class variable that describes the internal structures of the .WAV
		/// </summary>
		private WaveFile		m_Wavefile;

		/// <summary>
		/// Boolean for whether the .WAV should draw or not.  So that the control doesnt draw the .WAV until after it is read
		/// </summary>
		private bool			m_DrawWave = false;

		/// <summary>
		/// Filename string
		/// </summary>
		private string			m_Filename;

		/// <summary>
		/// Each pixel value (X direction) represents this many samples in the wavefile
		/// Starting value is based on control width so that the .WAV will cover the entire width.
		/// </summary>
		private double			m_SamplesPerPixel = 0.0;

		/// <summary>
		/// This value is the amount to increase/decrease the m_SamplesPerPixel.  This creates a 'Zoom' affect.
		/// Starting value is m_SamplesPerPixel / 25    so that it is scaled for the size of the .WAV
		/// </summary>
		private double			m_ZoomFactor;

		/// <summary>
		/// This is the starting x value of a mouse drag
		/// </summary>
		private int				m_StartX = 0;

		/// <summary>
		/// This is the ending x value of a mouse drag
		/// </summary>
		private int				m_EndX = 0;

		/// <summary>
		/// This is the value of the previous mouse move event
		/// </summary>
		private int				m_PrevX = 0;


		/// <summary>
		/// This boolean value gets rid of the currently active region and also refreshes the wave
		/// </summary>
		private bool			m_ResetRegion;

		/// <summary>
		/// Boolean for whether the Alt key is down
		/// </summary>
		private bool			m_AltKeyDown = false;

		/// <summary>
		/// Offset from the beginning of the wave for where to start drawing
		/// </summary>
		private int				m_OffsetInSamples = 0;

		public string Filename
		{
			set { m_Filename = value; }
			get { return m_Filename; }
		}

		private double SamplesPerPixel
		{
			set
			{
				m_SamplesPerPixel = value;
				m_ZoomFactor = m_SamplesPerPixel / 25;
			}
		}

		public WaveControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// sets up double buffering
			SetStyle(System.Windows.Forms.ControlStyles.UserPaint|System.Windows.Forms.ControlStyles.AllPaintingInWmPaint|System.Windows.Forms.ControlStyles.DoubleBuffer, true);
		}

		public void Read( )
		{
			m_Wavefile = new WaveFile( m_Filename );

			m_Wavefile.Read(88);
			
			m_DrawWave = true;

			Refresh( );
		}

        public void Draw(WaveFile wf)
        {
            m_Wavefile = wf;

            m_DrawWave = true;

            Refresh();
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // WaveControl
            // 
            this.BackColor = System.Drawing.Color.SkyBlue;
            this.Name = "WaveControl";
            this.Size = new System.Drawing.Size(616, 328);
            this.Load += new System.EventHandler(this.WaveControl_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.WaveControl_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WaveControl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.WaveControl_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WaveControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WaveControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WaveControl_MouseUp);
            this.ResumeLayout(false);

		}
		#endregion

		private void WaveControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Pen pen = new Pen( ForeColor );

			if ( m_DrawWave )
			{
				Draw( e, pen );
			}
			
			int regionStartX = Math.Min( m_StartX, m_EndX );
			int regionEndX = Math.Max( m_StartX, m_EndX );
			SolidBrush brush = new SolidBrush( Color.FromArgb( 128, 0, 0, 0 ) );
			e.Graphics.FillRectangle( brush, regionStartX, 0, regionEndX - regionStartX, e.Graphics.VisibleClipBounds.Height );
		}

		protected override void OnMouseWheel( MouseEventArgs mea )
		{
			if ( mea.Delta * SystemInformation.MouseWheelScrollLines / 120 > 0 )
				ZoomIn( );
			else
				ZoomOut( );

			Refresh( );
		}

		private void Draw( PaintEventArgs pea, Pen pen )
		{
			Graphics grfx = pea.Graphics;

			RectangleF visBounds = grfx.VisibleClipBounds;

			if ( m_SamplesPerPixel == 0.0 )
			{
				this.SamplesPerPixel = ( m_Wavefile.Data.NumSamples / visBounds.Width );
			}

			grfx.DrawLine( pen, 0, visBounds.Height / 2, visBounds.Width, visBounds.Height / 2 );

			grfx.TranslateTransform( 0, visBounds.Height );
			grfx.ScaleTransform( 1, -1 );

			if ( m_Wavefile.Format.BitsPerSample == 16 )
				Draw16Bit( grfx, pen, visBounds );
		}

		private void Draw16Bit( Graphics grfx, Pen pen, RectangleF visBounds )
		{
			int prevX = 0;
			int prevY = 0;

			int i = 0;

			// index is how far to offset into the data array
			int index = m_OffsetInSamples;
			int maxSampleToShow = (int) (( m_SamplesPerPixel * visBounds.Width ) + m_OffsetInSamples);

			maxSampleToShow = Math.Min( maxSampleToShow, m_Wavefile.Data.NumSamples );

			while ( index < maxSampleToShow )
			{
				short maxVal = -32767;
				short minVal = 32767;

				// finds the max & min peaks for this pixel 
				for ( int x = 0; x < m_SamplesPerPixel; x++ )
				{
					maxVal = Math.Max( maxVal, m_Wavefile.Data[ x + index ] );
					minVal = Math.Min( minVal, m_Wavefile.Data[ x + index ] );
				}

				// scales based on height of window
				int scaledMinVal = (int) (( (minVal + 32768) * visBounds.Height ) / 65536 );
				int scaledMaxVal = (int) (( (maxVal + 32768) * visBounds.Height ) / 65536 );

				//  if samples per pixel is small or less than zero, we are out of zoom range, so don't display anything
				if ( m_SamplesPerPixel > 0.0000000001 )
				{
					// if the max/min are the same, then draw a line from the previous position,
					// otherwise we will not see anything
					if ( scaledMinVal == scaledMaxVal )
					{
						if ( prevY != 0 )
							grfx.DrawLine( pen, prevX, prevY, i, scaledMaxVal );
					}
					else
					{
						grfx.DrawLine( pen, i, scaledMinVal, i, scaledMaxVal );
					}
				}
				else
					return;

				prevX = i;
				prevY = scaledMaxVal;
				
				i++;
				index = (int) ( i * m_SamplesPerPixel) + m_OffsetInSamples;
			}
		}

		private void ZoomIn( )
		{
			m_SamplesPerPixel -= m_ZoomFactor;
		}

		private void ZoomOut( )
		{
			m_SamplesPerPixel += m_ZoomFactor;
		}

		private void ZoomToRegion( )
		{
			int regionStartX = Math.Min( m_StartX, m_EndX );
			int regionEndX = Math.Max( m_StartX, m_EndX );

			// if they are negative, make them zero
			regionStartX = Math.Max( 0, regionStartX );
			regionEndX = Math.Max( 0, regionEndX );

			m_OffsetInSamples += (int) (regionStartX * m_SamplesPerPixel);

			int numSamplesToShow = (int) (( regionEndX - regionStartX ) * m_SamplesPerPixel);

			if ( numSamplesToShow > 0 )
			{
				this.SamplesPerPixel = (double) numSamplesToShow / this.Width;;

				m_ResetRegion = true;
			}
		}

		private void ZoomOutFull( )
		{
			this.SamplesPerPixel = ( m_Wavefile.Data.NumSamples / this.Width );
			m_OffsetInSamples = 0;

			m_ResetRegion = true;
		}

		private void Scroll( int newXValue )
		{
			m_OffsetInSamples -= (int) ( ( newXValue - m_PrevX ) * m_SamplesPerPixel );

			if ( m_OffsetInSamples < 0 )
				m_OffsetInSamples = 0;
		}

		private void WaveControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( e.Button == MouseButtons.Left )
			{
				if ( m_AltKeyDown )
				{
					m_PrevX = e.X;
				}
				else
				{
					m_StartX = e.X;
					m_ResetRegion = true;
				}
			}
			else if ( e.Button == MouseButtons.Right )
			{
				if ( e.Clicks == 2 )
					ZoomOutFull( );
				else
					ZoomToRegion( );
			}
		}

		private void WaveControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( e.Button == MouseButtons.Left )
			{
				if ( m_AltKeyDown )
				{
					Scroll( e.X );
				}
				else
				{
					m_EndX = e.X;
					m_ResetRegion = false;
				}

				m_PrevX = e.X;

				Refresh( );
			}
		}

		private void WaveControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( m_AltKeyDown )
				return;

			if ( m_ResetRegion )
			{
				m_StartX = 0;
				m_EndX = 0;

				Refresh( );
			}
			else
			{
				m_EndX = e.X;
			}
		}

		private void WaveControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if ( e.Alt )
			{
				m_AltKeyDown = true;
			}
		}

		private void WaveControl_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if ( e.KeyCode == Keys.Menu )
			{
				m_AltKeyDown = false;
			}
		}

        private void WaveControl_Load(object sender, EventArgs e)
        {

        }
	}
}
