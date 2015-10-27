﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SomnoSoftware.Control;

namespace SomnoSoftware
{
    class ShowSpectrogram : Show 
    {           
        // Position Variables for Spectrogram
        float faxispos_x = 60;      // x-position freqency axis
        float faxispos_ytop = 30;   // y-distance frequency axis on top
        float faxispos_ybot = 40;   // y-distance freqency axis on bottom

        float taxispos_xr = 15;     // x-position time axis right

        float taxis_length;
        float faxis_length;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pb">referece variable on picturebox for spectrogram (pb_spec)</param>
        public ShowSpectrogram(ref PictureBox pb)
        {
            h = pb.Height;
            w = pb.Width;            
            taxis_length = w - faxispos_x - taxispos_xr;  // Length of t-axis in pixels
            faxis_length = h - faxispos_ybot - faxispos_ytop;  // Length of f-axis in pixels
            
            this.pb = pb;
            bmp_front = new Bitmap(pb.Width, pb.Height);
            bmp_back = new Bitmap(pb.Width, pb.Height);
           
            InitializeSpectrogram();
        }


        /// <summary>
        /// Draw axis for spectrogram
        /// </summary>
        private void InitializeSpectrogram()
        {
            Graphics g = Graphics.FromImage(bmp_back);           
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            
            string[] ftick_label = new string[5]{"2","1,5","1","0,5","0"};
            string[] ttick_label = new string[6]{"0", "2", "4", "6", "8", "10"};
            
            float taxispos_y = h-faxispos_ybot;

            g.DrawRectangle(p_frame, new Rectangle(1, 1, w - 2, h - 2));

            ///////// frequency axis ////////////////////            
            
            g.DrawLine(p_black, faxispos_x , faxispos_ytop, faxispos_x, h - faxispos_ybot);         // frequency axis            

            for (int i = 0; i < ftick_label.Length; i++)                                           // ticks and their labels on frequency axis 
            {
                float ftickpos_y = faxispos_ytop + i * (h - (faxispos_ybot + faxispos_ytop)) / (ftick_label.Length - 1);
                g.DrawLine(p_black, faxispos_x-3, ftickpos_y, faxispos_x+3, ftickpos_y);
                g.DrawString(ftick_label[i], font, b_black, faxispos_x-30 , ftickpos_y-7);
            }
                     
            g.RotateTransform(270);                                                             // label
            g.DrawString("Frequenz [kHz]", font, b_black, -((h / 2) + 40), faxispos_x - 50);
            g.ResetTransform();

            ///////// time axis ////////////////////

            g.DrawLine(p_black, faxispos_x, taxispos_y, w - taxispos_xr, taxispos_y);      // time axis

            for (int i = 0; i < ttick_label.Length; i++)
            {
                float ttickpos_x = faxispos_x + i * (w - (faxispos_x + taxispos_xr)) / (ttick_label.Length - 1);
                g.DrawLine(p_black, ttickpos_x, taxispos_y - 3, ttickpos_x, taxispos_y + 3);
                g.DrawString(ttick_label[i], font, b_black, ttickpos_x - 5, taxispos_y + 8);
            }


            g.DrawString("Zeit [s]", font, b_black, ((w - faxispos_x - taxispos_xr) / 2) + faxispos_x - 20, (taxispos_y) + 20);              // label

            g.DrawString("Spectrogramm (" + Statics.FFTSize.ToString() + " Datenpunkte)", font, b_black, ((w - faxispos_x - taxispos_xr) / 2) + faxispos_x - 80, 10);

            pb.BackgroundImage = bmp_back;
        }


        /// <summary>
        /// Draw current spectral line
        /// </summary>
        /// <param name="FFT">64 sample spectrum of current data</param>
        public void DrawSpectrogram(double[] FFT, int counter)
        {
            Graphics g = Graphics.FromImage(bmp_front);
                        
            float x = 0;
            float y = 0;
            
            float box_width = taxis_length / Statics.num_of_lines;      // width of single box

            float limit = 2000 / (Statics.FS / Statics.FFTSize);        // number of boxes ploted until 2000 Hz
            float box_height = faxis_length / limit;                    // height of single box
            

            for (int i = 0; i < (int)limit; i++)
            {
                Color c = MapRainbowColor((float)FFT[i], 40, 8);
                b_black.Color = c;
                x = faxispos_x + counter * box_width + 1;               // x-position box
                y = (h - faxispos_ybot) - (i + 1) * box_height ;        // y-position box       

                g.FillRectangle(b_black, new RectangleF(x, y, box_width, box_height));    // Draw box          

            }

            if (counter < Statics.num_of_lines - 2)                 // avoid black line at the end of spectrogram
            {  
                b_black.Color = Color.Black;
                g.FillRectangle(b_black, new RectangleF(x + box_width, faxispos_ytop, 3, faxis_length));
            }

            pb.Image = bmp_front;

        }


        /// <summary>
        /// Determines Color based on float input value
        /// </summary>
        /// <param name="value">value for coverting in color</param>
        /// <param name="red_value">value that represents red color (maximum)</param>
        /// <param name="blue_value">value that represents blue color (minimum)</param>
        /// <returns>Color</returns>
        private Color MapRainbowColor(float value, float red_value, float blue_value)
        {

            if (value > red_value)
                value = red_value;

            if (value < blue_value)
                value = blue_value;            
            
            // Convert into a value between 0 and 1023.
            int int_value = (int)(1023 * (value - red_value) / (blue_value - red_value));

            // Map different color bands.
            if (int_value < 256)
            {
                // Red to yellow. (255, 0, 0) to (255, 255, 0).
                return Color.FromArgb(255, int_value, 0);
            }
            else if (int_value < 512)
            {
                // Yellow to green. (255, 255, 0) to (0, 255, 0).
                int_value -= 256;
                return Color.FromArgb(255 - int_value, 255, 0);
            }
            else if (int_value < 768)
            {
                // Green to aqua. (0, 255, 0) to (0, 255, 255).
                int_value -= 512;
                return Color.FromArgb(0, 255, int_value);
            }
            else
            {
                // Aqua to blue. (0, 255, 255) to (0, 0, 255).
                int_value -= 768;
                return Color.FromArgb(0, 255 - int_value, 255);
            }
        }



    }
}
