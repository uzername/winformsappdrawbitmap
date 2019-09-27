using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsAppDrawBitmap
{
    public partial class Form1 : Form
    {
        public UserControl ctrlToDraw;
        private Bitmap bitmapInitial;
        private Bitmap bitmapRender;
        public double currentscalefactor = 1.0; public double scaledelta = 0.1;
        public double currentanglevalue = 0.0;
        bool triggerscaleChangedEvt = true; bool triggerrotationChangedEvt = true;
        public Form1()
        {
            InitializeComponent();
            ctrlToDraw = new UserControl();
            ctrlToDraw.Dock = DockStyle.Fill;
            ctrlToDraw.BackColor = Color.White;
            ctrlToDraw.Paint += CtrlToDraw_Paint;
            tableLayoutPanel1.Controls.Add(ctrlToDraw, 0, 0);
            InitializeBitmaps();
            triggerscaleChangedEvt = false;
            numericUpDownScaleFactor.Value = (decimal)currentscalefactor;
            triggerscaleChangedEvt = true;
            ctrlToDraw.Refresh();
        }

        private void InitializeBitmaps()
        {
            bitmapInitial = new Bitmap(100, 100);
            Pen blackPen = new Pen(Color.Black,2.0f);
            Graphics g = Graphics.FromImage(bitmapInitial);
            g.DrawArc(blackPen, 5, 5, 20, 20, 45, 90);
            g.DrawLine(blackPen, 40, 0, 30, 100);
            g.DrawLine(blackPen, 10, 2, 90, 2);
            g.DrawLine(blackPen, 90, 2, 90, 95);
            g.DrawLine(blackPen, 10, 95, 90, 95);
            g.DrawLine(blackPen, 10, 95, 10, 2);

            bitmapRender = new Bitmap(bitmapInitial);            
        }

        private void CtrlToDraw_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmapRender, new Point(0, 0));
        }
        public double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }
        public double ConvertRadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }
        // https://stackoverflow.com/questions/2163829/how-do-i-rotate-a-picture-in-winforms
        private void rescaleImageUsingCurrentScaleFactor() {
            double newWidthPrescaled = currentscalefactor * bitmapInitial.Width;
            double newHeightPrescaled = currentscalefactor * bitmapInitial.Height;
           
            bitmapRender = new Bitmap((int)(newWidthPrescaled),(int)(newHeightPrescaled));
            using (Graphics ee = Graphics.FromImage(bitmapRender))
            {
                ee.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                ee.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                ee.DrawImage(bitmapInitial, 0, 0, (float)(newWidthPrescaled), (float)(newHeightPrescaled));

            }                                        
        }

        private Bitmap RotateImageUsingCurrentRotationAngle()
        {
            //First, re-center the image in a larger image that has a margin/frame
            //to compensate for the rotated image's increased size
            float angle = (float) currentanglevalue;
            Bitmap rotateMe = bitmapRender;
            double ALPHA_DEG = currentanglevalue;
            double BETA_DEG = 90 - currentanglevalue;
            double ALPHA_RAD = ConvertDegreesToRadians(ALPHA_DEG);
            double BETA_RAD = ConvertDegreesToRadians(BETA_DEG);
            double newHeight = Math.Abs(Math.Abs(rotateMe.Height * Math.Sin(ALPHA_RAD)) + Math.Abs(rotateMe.Width * Math.Sin(BETA_RAD)));
            double newWidth = Math.Abs(Math.Abs(rotateMe.Height * Math.Cos(ALPHA_RAD)) + Math.Abs(rotateMe.Width * Math.Cos(BETA_RAD)));
            double offsetHeight = (newHeight - rotateMe.Height) / 2.0;
            double offsetWidth = (newWidth -  rotateMe.Width) / 2.0;
            System.Diagnostics.Debug.Assert(offsetHeight >= 0);
            System.Diagnostics.Debug.Assert(offsetWidth >= 0);
            var bmp = new Bitmap((int)newWidth, (int)newHeight);

            using (Graphics g = Graphics.FromImage(bmp))
                g.DrawImageUnscaled(rotateMe, (int)offsetWidth, (int)offsetHeight, bmp.Width, bmp.Height);

            rotateMe = bmp;

            //Now, actually rotate the image
            Bitmap rotatedImage = new Bitmap(rotateMe.Width, rotateMe.Height);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(rotateMe.Width / 2, rotateMe.Height / 2);   //set the rotation point as the center into the matrix
                g.RotateTransform(angle);                                        //rotate
                g.TranslateTransform(-rotateMe.Width / 2, -rotateMe.Height / 2); //restore rotation point into the matrix
                g.DrawImage(rotateMe, new Point(0, 0));                          //draw the image on the new bitmap
            }

            return rotatedImage;
        }

        private void numericUpDownScaleFactor_ValueChanged(object sender, EventArgs e)
        {
            if (triggerscaleChangedEvt == false) return;
            this.currentscalefactor = (double) numericUpDownScaleFactor.Value;
            rescaleImageUsingCurrentScaleFactor();
            this.bitmapRender = RotateImageUsingCurrentRotationAngle();
            ctrlToDraw.Refresh();
        }

        private void numericUpDownAngle_ValueChanged(object sender, EventArgs e)
        {
            if (triggerrotationChangedEvt == false) return;
            this.currentanglevalue = (double)numericUpDownAngle.Value;
            rescaleImageUsingCurrentScaleFactor();
            this.bitmapRender = RotateImageUsingCurrentRotationAngle();
            ctrlToDraw.Refresh();
        }
    }
}
