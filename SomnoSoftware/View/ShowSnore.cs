using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SomnoSoftware
{
    class ShowSnore : Show
    {
        private int counter = 0;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pb">reference of picturebox for position display</param>
        public ShowSnore(ref PictureBox pb)
        {
            this.pb = pb;
            
            h = pb.Height;
            w = pb.Width;        
  
            bmp_front = new Bitmap(pb.Width, pb.Height);
            bmp_back = new Bitmap(pb.Width, pb.Height);            
           
            Initialize();
        }

        /// <summary>
        /// Draw font and frame
        /// </summary>
        private void Initialize()
        {
            Graphics g = Graphics.FromImage(bmp_back);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //g.DrawString("Schlaflage", font, b_black, new PointF(w / 2 - 30, 5));

            g.DrawRectangle(p_frame, new Rectangle(1, 1, w - 2, h - 2));

            pb.BackgroundImage = bmp_back;
    
        }

        /// <summary>
        /// Draw Image and font
        /// </summary>
        /// <param name="pos">position value (0...2)</param>
        public void DrawPosition(bool snore)
        {
            Graphics g = Graphics.FromImage(bmp_front);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            pb.Image = null;                    // Clear Image before redrawing
            g.Clear(Color.Transparent);

            float pic_w = w-8;                   // Width of image
            float pic_h = h-5;                  // Height of image    

            float pic_x = 4;                   // x-position of image
            float pic_y = h / 2 - pic_h / 2;    // y-position of image

            //int font_x = w / 2 - 22;              // x-position of font
            //int font_y = h - 30;                 // y-position of font

            if (snore) counter = 20;

            if (counter > 0)
            {
                g.DrawImage(Properties.Resources.snoring, new RectangleF(pic_x, pic_y, pic_w, pic_h));
                counter--;
            }
            else
            {
                g.Clear(Color.Transparent);
            }

            pb.Image = bmp_front;

        }

    }
}
