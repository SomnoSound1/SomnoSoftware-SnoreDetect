using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomnoSoftware.Model
{
    class ProcessSnore
    {
        private double yf=0;                           //filtered value
        private double y340 = 0;                       //filtered value
        private double y53 = 0;                        //filtered value

        private double[] Abp = new double[7] { 1, -3.2930, 5.6655, -5.9137, 4.0384, -1.6666, 0.3618 };        //declare databuffer bp
        private double[] Bbp = new double[7] { 0.0102, 0, -0.0305, 0, 0.0305, 0, -0.0102 };        //declare databuffer bp

        private double[] Alp53 = new double[3] { 1,-0.9999,0};
        private double[] Blp53 = new double[3] { 0.000033, 0.000033, 0 };

        private double[] Alp340 = new double[3] { 1, -0.9996, 0 };
        private double[] Blp340 = new double[3] { 0.000213, 0.000213, 0 };

        private List<double> yseg = new List<double>();
        private bool max = false;                    //segmentation declarations
        private bool min = false;
        private double c_min = 10;
        private double c_max = 0;
        private int i_min = 0;
        private int segment = 0;

        //fft
        int stepsize = Statics.FS / Statics.FFTSize;
        private List<double[]> yfft = new List<double[]>();

        private int silence = 4;                    //decision tree declarations
        private int breathing = 15;         
        private int A0=150000;                      //integral boundaries
        private int A2=8000;                        //length boundaries
        private int A3=17000;
        
        private double[] bpbuffer = new double[7];        //declare databuffer bp
        private double[] lpbuffer340 = new double[3];     //declare databuffer lp 340mHz
        private double[] lpbuffer53 = new double[3];      //declare databuffer lp 53mHz

        public int length = 0;
        public int volume = 0;
        public double freq = 0;
        
        public bool DetectSnoring(double value, double[] fft)
        {
            
            yf=Filter(Bbp, Abp, value, ref bpbuffer);
            yf=Math.Abs(yf);
            y340 = Filter(Blp340, Alp340, yf, ref lpbuffer340);
            //y53 = Filter(Blp53, Alp53, yf, ref lpbuffer53);

            if (yseg.Count > 20000)
            {
                yseg.Clear();
                yfft.Clear();
            }

            //segmentation
            double x = y340;
            if (!min)
            {
                if (x < c_min)
                {
                    c_min = x;
                    yfft.Clear();
                    yfft.Add(fft);
                    yseg.Clear();
                    yseg.Add(x);
                }
                else
                {
                    yfft.Add(fft);
                    yseg.Add(x);
                }

                if (x > 1.9*c_min)
                {
                    min = true;
                    c_max = x;
                }
            }
            else
            {
                yfft.Add(fft);
                yseg.Add(x);
                
                if (x > c_max)
                {
                    c_max = x;
                }

                if (x < 0.5*c_max)
                {

                    min = false;
                    c_min = x;

                    double fftpower = (yfft.Sum(item => item.Sum())) / fft.Length;
                    double fftlow = 0;
                    foreach (double[] item in yfft)
                    {
                        for (int k = stepsize; k < 250; k+=stepsize)
                        {
                            fftlow += item[k/stepsize - 1];
                        }
                    }
                    fftlow = fftlow/(250/stepsize);

                    //plot values
                    length = yseg.Count;
                    volume = Convert.ToInt32(yseg.Sum());
                    //volume = Convert.ToInt32(yseg.Max());
                    freq = Convert.ToInt32(fftlow / yseg.Count);

                    double sens = Statics.snoreSens;


                    if (yseg.Sum() > (A0 * sens) && (yseg.Count > A2) && (yseg.Count < A3) &&  fftlow / yseg.Count > 30 * sens)
                    {
                        return true;
                    }

                    //decision tree 
                    //if (yseg.Sum() > (A0*sens) && (yseg.Count > A2) && (yseg.Count < A3) && (fftlow / fftpower) > 1.5 && fftlow / yseg.Count > 32*sens)
                    //{
                    //    return true;
                    //}
                    
                }
            }

            return false;
        }


        private double Filter(double[] B, double[] A, double value, ref double[] buffer)
        {
            //shift
            for (int k = 0; k < (B.Length-1); k++)
            {
                buffer[k] = buffer[k+1];
            }
            buffer[B.Length-1] = 0;

            //numerator
            for (int k = 0; k < B.Length; k++)
            {
                buffer[k] = buffer[k] + value * B[k];
            }

            //denumerator
            for (int k = 0; k < (B.Length-1); k++)
            {
                buffer[k + 1] = buffer[k + 1] - buffer[0] * A[k + 1];
            }

            return buffer[0];
        }


    }
}
