using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

using System.IO;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Canny CannyData;


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            String Img = openFileDialog1.FileName;
            Bitmap originalImg = (Bitmap)Image.FromFile(Img);
            Bitmap resizedImg = new Bitmap(originalImg, new Size(400, 400));
            pictureBox1.Image = resizedImg;
            pictureBox3.Image = resizedImg;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dt1 = new DateTime();
            DateTime dt2 = new DateTime();
            TimeSpan dt3 = new TimeSpan();
            float TH, TL, Sigma;
            int MaskSize;

            dt1 = DateTime.Now;
            TH = 20;
            TL = 10;

            MaskSize = 5;
            Sigma = 1;
            CannyData = new Canny((Bitmap)pictureBox1.Image, TH, TL, MaskSize, Sigma);

            pictureBox4.Image = CannyData.DisplayImage(CannyData.NonMax);

           // pictureBox2.Image = CannyData.DisplayImage(CannyData.FilteredImage);

            //GNL.Image = CannyData.DisplayImage(CannyData.GNL);

            //GNH.Image = CannyData.DisplayImage(CannyData.GNH);

            // pictureBox4.Image = CannyData.DisplayImage(CannyData.EdgeMap);

            
        
    }

        private void button3_Click(object sender, EventArgs e)
        {
            Color OkunanRenk;
            Bitmap GirisResmi, CikisResmiX, CikisResmiY, CikisResmiXY;
            GirisResmi = new Bitmap(pictureBox1.Image);
            int ResimGenisligi = GirisResmi.Width;
            int ResimYuksekligi = GirisResmi.Height;
            CikisResmiX = new Bitmap(ResimGenisligi, ResimYuksekligi);
            CikisResmiY = new Bitmap(ResimGenisligi, ResimYuksekligi);
            CikisResmiXY = new Bitmap(ResimGenisligi, ResimYuksekligi);
            int SablonBoyutu = 3;
            int ElemanSayisi = SablonBoyutu * SablonBoyutu;
            int x, y, i, j;
            int Gri = 0;
            int[] MatrisX = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
            int[] MatrisY = { 1, 2, 1, 0, 0, 0, -1, -2, -1 };
            int RenkX, RenkY, RenkXY;

            for (x = (SablonBoyutu - 1) / 2; x < ResimGenisligi - (SablonBoyutu - 1) / 2; x++) //Resmi
            {
                for (y = (SablonBoyutu - 1) / 2; y < ResimYuksekligi - (SablonBoyutu - 1) / 2; y++)
                {
                    int toplamGriX = 0, toplamGriY = 0;
                    //Şablon bölgesi (çekirdek matris) içindeki pikselleri tarıyor.
                    int k = 0; //matris içindeki elemanları sırayla okurken kullanılacak.
                    for (i = -((SablonBoyutu - 1) / 2); i <= (SablonBoyutu - 1) / 2; i++)
                    {
                        for (j = -((SablonBoyutu - 1) / 2); j <= (SablonBoyutu - 1) / 2; j++)
                        {
                            OkunanRenk = GirisResmi.GetPixel(x + i, y + j);
                            Gri = (OkunanRenk.R + OkunanRenk.G + OkunanRenk.B) / 3;
                            toplamGriX = toplamGriX + Gri * MatrisX[k];
                            toplamGriY = toplamGriY + Gri * MatrisY[k];
                            k++;
                        }
                    }
                    RenkX = Math.Abs(toplamGriX);
                    RenkY = Math.Abs(toplamGriY);
                    RenkXY = Math.Abs(toplamGriX) + Math.Abs(toplamGriY);
                    //===========================================================
                    //Renkler sınırların dışına çıktıysa, sınır değer alınacak.
                    if (RenkX > 255) RenkX = 255;
                    if (RenkY > 255) RenkY = 255;
                    if (RenkXY > 255) RenkXY = 255;
                    if (RenkX < 0) RenkX = 0;
                    if (RenkY < 0) RenkY = 0;
                    if (RenkXY < 0) RenkXY = 0;
                    //===========================================================
                    CikisResmiX.SetPixel(x, y, Color.FromArgb(RenkX, RenkX, RenkX));
                    CikisResmiY.SetPixel(x, y, Color.FromArgb(RenkY, RenkY, RenkY));
                    CikisResmiXY.SetPixel(x, y, Color.FromArgb(RenkXY, RenkXY, RenkXY));
                }
            }
            pictureBox2.Image = CikisResmiXY;
            //pictureBox3.Image = CikisResmiY;
            //pictureBox4.Image = CikisResmiXY;
        }
    }


    internal class Canny
        {
            public int Width, Height;
            public Bitmap Obj;
            public int[,] GreyImage;
            //Gaussian Kernel Data
            int[,] GaussianKernel;
            int KernelWeight;
            int KernelSize = 5;
            float Sigma = 1;   // for N=2 Sigma =0.85  N=5 Sigma =1, N=9 Sigma = 2    2*Sigma = (int)N/2
                               //Canny Edge Detection Parameters
            float MaxHysteresisThresh, MinHysteresisThresh;
            public float[,] DerivativeX;
            public float[,] DerivativeY;
            public int[,] FilteredImage;
            public float[,] Gradient;
            public float[,] NonMax;
            public int[,] PostHysteresis;
            int[,] EdgePoints;
            public float[,] GNH;
            public float[,] GNL;
            public int[,] EdgeMap;
            public int[,] VisitedMap;

            public Canny(Bitmap Input)
            {
                // Gaussian and Canny Parameters
                MaxHysteresisThresh = 20F;
                MinHysteresisThresh = 10F;
                Obj = Input;
                Width = Obj.Width;
                Height = Obj.Height;
                EdgeMap = new int[Width, Height];
                VisitedMap = new int[Width, Height];

                ReadImage();
                DetectCannyEdges();
                return;
            }

            public Canny(Bitmap Input, float Th, float Tl)
            {

                // Gaussian and Canny Parameters

                MaxHysteresisThresh = Th;
                MinHysteresisThresh = Tl;

                Obj = Input;
                Width = Obj.Width;
                Height = Obj.Height;

                EdgeMap = new int[Width, Height];
                VisitedMap = new int[Width, Height];

                ReadImage();
                DetectCannyEdges();
                return;
            }

            public Canny(Bitmap Input, float Th, float Tl, int GaussianMaskSize, float SigmaforGaussianKernel)
            {

                // Gaussian and Canny Parameters

                MaxHysteresisThresh = Th;
                MinHysteresisThresh = Tl;
                KernelSize = GaussianMaskSize;
                Sigma = SigmaforGaussianKernel;
                Obj = Input;
                Width = Obj.Width;
                Height = Obj.Height;

                EdgeMap = new int[Width, Height];
                VisitedMap = new int[Width, Height];

                ReadImage();
                DetectCannyEdges();
                return;
            }

            public Bitmap DisplayImage()
            {
                int i, j;
                Bitmap image = new Bitmap(Obj.Width, Obj.Height);
                BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, Obj.Width, Obj.Height),
                                         ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
                {
                    byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                    for (i = 0; i < bitmapData1.Height; i++)
                    {
                        for (j = 0; j < bitmapData1.Width; j++)
                        {
                            // write the logic implementation here
                            imagePointer1[0] = (byte)GreyImage[j, i];
                            imagePointer1[1] = (byte)GreyImage[j, i];
                            imagePointer1[2] = (byte)GreyImage[j, i];
                            imagePointer1[3] = (byte)255;
                            //4 bytes per pixel
                            imagePointer1 += 4;
                        }//end for j

                        //4 bytes per pixel
                        imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                    }//end for i
                }//end unsafe
                image.UnlockBits(bitmapData1);
                return image;// col;
            }      // Display Grey Image

            public Bitmap DisplayImage(float[,] GreyImage)
            {
                int i, j;
                int W, H;
                W = GreyImage.GetLength(0);
                H = GreyImage.GetLength(1);
                Bitmap image = new Bitmap(W, H);
                BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, W, H),
                                         ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {

                    byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                    for (i = 0; i < bitmapData1.Height; i++)
                    {
                        for (j = 0; j < bitmapData1.Width; j++)
                        {
                            // write the logic implementation here
                            imagePointer1[0] = (byte)((int)(GreyImage[j, i]));
                            imagePointer1[1] = (byte)((int)(GreyImage[j, i]));
                            imagePointer1[2] = (byte)((int)(GreyImage[j, i]));
                            imagePointer1[3] = (byte)255;
                            //4 bytes per pixel
                            imagePointer1 += 4;
                        }   //end for j
                            //4 bytes per pixel
                        imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                    }//End for i
                }//end unsafe
                image.UnlockBits(bitmapData1);
                return image;// col;
            }      // Display Grey Imag

            public Bitmap DisplayImage(int[,] GreyImage)
            {
                int i, j;
                int W, H;
                W = GreyImage.GetLength(0);
                H = GreyImage.GetLength(1);
                Bitmap image = new Bitmap(W, H);
                BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, W, H),
                                         ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {

                    byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                    for (i = 0; i < bitmapData1.Height; i++)
                    {
                        for (j = 0; j < bitmapData1.Width; j++)
                        {
                            // write the logic implementation here
                            imagePointer1[0] = (byte)GreyImage[j, i];
                            imagePointer1[1] = (byte)GreyImage[j, i];
                            imagePointer1[2] = (byte)GreyImage[j, i];
                            imagePointer1[3] = (byte)255;
                            //4 bytes per pixel
                            imagePointer1 += 4;
                        }   //end for j
                            //4 bytes per pixel
                        imagePointer1 += (bitmapData1.Stride - (bitmapData1.Width * 4));
                    }//End for i
                }//end unsafe
                image.UnlockBits(bitmapData1);
                return image;// col;
            }      // Display Grey Image

            private void ReadImage()
            {
                int i, j;
                GreyImage = new int[Obj.Width, Obj.Height];  //[Row,Column]
                Bitmap image = Obj;
                BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                         ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                    for (i = 0; i < bitmapData1.Height; i++)
                    {
                        for (j = 0; j < bitmapData1.Width; j++)
                        {
                            GreyImage[j, i] = (int)((imagePointer1[0] + imagePointer1[1] + imagePointer1[2]) / 3.0);
                            //4 bytes per pixel
                            imagePointer1 += 4;
                        }//end for j
                         //4 bytes per pixel
                        imagePointer1 += bitmapData1.Stride - (bitmapData1.Width * 4);
                    }//end for i
                }//end unsafe
                image.UnlockBits(bitmapData1);
                return;
            }

            private void GenerateGaussianKernel(int N, float S, out int Weight)
            {

                float Sigma = S;
                float pi;
                pi = (float)Math.PI;
                int i, j;
                int SizeofKernel = N;

                float[,] Kernel = new float[N, N];
                GaussianKernel = new int[N, N];
                float[,] OP = new float[N, N];
                float D1, D2;


                D1 = 1 / (2 * pi * Sigma * Sigma);
                D2 = 2 * Sigma * Sigma;

                float min = 1000;

                for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
                {
                    for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                    {
                        Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = ((1 / D1) * (float)Math.Exp(-(i * i + j * j) / D2));
                        if (Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] < min)
                            min = Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];

                    }
                }
                int mult = (int)(1 / min);
                int sum = 0;
                if ((min > 0) && (min < 1))
                {

                    for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
                    {
                        for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                        {
                            Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (float)Math.Round(Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] * mult, 0);
                            GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (int)Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                            sum = sum + GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                        }

                    }

                }
                else
                {
                    sum = 0;
                    for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
                    {
                        for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                        {
                            Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (float)Math.Round(Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j], 0);
                            GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (int)Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                            sum = sum + GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                        }

                    }

                }
                //Normalizing kernel Weight
                Weight = sum;

                return;
            }

            private int[,] GaussianFilter(int[,] Data)
            {
                GenerateGaussianKernel(KernelSize, Sigma, out KernelWeight);

                int[,] Output = new int[Width, Height];
                int i, j, k, l;
                int Limit = KernelSize / 2;

                float Sum = 0;


                Output = Data; // Removes Unwanted Data Omission due to kernel bias while convolution


                for (i = Limit; i <= ((Width - 1) - Limit); i++)
                {
                    for (j = Limit; j <= ((Height - 1) - Limit); j++)
                    {
                        Sum = 0;
                        for (k = -Limit; k <= Limit; k++)
                        {

                            for (l = -Limit; l <= Limit; l++)
                            {
                                Sum = Sum + ((float)Data[i + k, j + l] * GaussianKernel[Limit + k, Limit + l]);

                            }
                        }
                        Output[i, j] = (int)(Math.Round(Sum / (float)KernelWeight));
                    }

                }


                return Output;
            }

            private float[,] Differentiate(int[,] Data, int[,] Filter)
            {
                int i, j, k, l, Fh, Fw;

                Fw = Filter.GetLength(0);
                Fh = Filter.GetLength(1);
                float sum = 0;
                float[,] Output = new float[Width, Height];

                for (i = Fw / 2; i <= (Width - Fw / 2) - 1; i++)
                {
                    for (j = Fh / 2; j <= (Height - Fh / 2) - 1; j++)
                    {
                        sum = 0;
                        for (k = -Fw / 2; k <= Fw / 2; k++)
                        {
                            for (l = -Fh / 2; l <= Fh / 2; l++)
                            {
                                sum = sum + Data[i + k, j + l] * Filter[Fw / 2 + k, Fh / 2 + l];


                            }
                        }
                        Output[i, j] = sum;

                    }

                }
                return Output;

            }

            private void DetectCannyEdges()
            {
                Gradient = new float[Width, Height];
                NonMax = new float[Width, Height];
                PostHysteresis = new int[Width, Height];

                DerivativeX = new float[Width, Height];
                DerivativeY = new float[Width, Height];

                //Gaussian Filter Input Image 

                FilteredImage = GaussianFilter(GreyImage);
                //Sobel Masks
                int[,] Dx = {{1,0,-1},
                         {1,0,-1},
                         {1,0,-1}};

                int[,] Dy = {{1,1,1},
                         {0,0,0},
                         {-1,-1,-1}};


                DerivativeX = Differentiate(FilteredImage, Dx);
                DerivativeY = Differentiate(FilteredImage, Dy);

                int i, j;

                //Compute the gradient magnitude based on derivatives in x and y:
                for (i = 0; i <= (Width - 1); i++)
                {
                    for (j = 0; j <= (Height - 1); j++)
                    {
                        Gradient[i, j] = (float)Math.Sqrt((DerivativeX[i, j] * DerivativeX[i, j]) + (DerivativeY[i, j] * DerivativeY[i, j]));

                    }

                }
                // Perform Non maximum suppression:
                // NonMax = Gradient;

                for (i = 0; i <= (Width - 1); i++)
                {
                    for (j = 0; j <= (Height - 1); j++)
                    {
                        NonMax[i, j] = Gradient[i, j];
                    }
                }

                int Limit = KernelSize / 2;
                int r, c;
                float Tangent;


                for (i = Limit; i <= (Width - Limit) - 1; i++)
                {
                    for (j = Limit; j <= (Height - Limit) - 1; j++)
                    {

                        if (DerivativeX[i, j] == 0)
                            Tangent = 90F;
                        else
                            Tangent = (float)(Math.Atan(DerivativeY[i, j] / DerivativeX[i, j]) * 180 / Math.PI); //rad to degree



                        //Horizontal Edge
                        if (((-22.5 < Tangent) && (Tangent <= 22.5)) || ((157.5 < Tangent) && (Tangent <= -157.5)))
                        {
                            if ((Gradient[i, j] < Gradient[i, j + 1]) || (Gradient[i, j] < Gradient[i, j - 1]))
                                NonMax[i, j] = 0;
                        }


                        //Vertical Edge
                        if (((-112.5 < Tangent) && (Tangent <= -67.5)) || ((67.5 < Tangent) && (Tangent <= 112.5)))
                        {
                            if ((Gradient[i, j] < Gradient[i + 1, j]) || (Gradient[i, j] < Gradient[i - 1, j]))
                                NonMax[i, j] = 0;
                        }

                        //+45 Degree Edge
                        if (((-67.5 < Tangent) && (Tangent <= -22.5)) || ((112.5 < Tangent) && (Tangent <= 157.5)))
                        {
                            if ((Gradient[i, j] < Gradient[i + 1, j - 1]) || (Gradient[i, j] < Gradient[i - 1, j + 1]))
                                NonMax[i, j] = 0;
                        }

                        //-45 Degree Edge
                        if (((-157.5 < Tangent) && (Tangent <= -112.5)) || ((67.5 < Tangent) && (Tangent <= 22.5)))
                        {
                            if ((Gradient[i, j] < Gradient[i + 1, j + 1]) || (Gradient[i, j] < Gradient[i - 1, j - 1]))
                                NonMax[i, j] = 0;
                        }

                    }
                }


                //PostHysteresis = NonMax;
                for (r = Limit; r <= (Width - Limit) - 1; r++)
                {
                    for (c = Limit; c <= (Height - Limit) - 1; c++)
                    {

                        PostHysteresis[r, c] = (int)NonMax[r, c];
                    }

                }

                //Find Max and Min in Post Hysterisis
                float min, max;
                min = 100;
                max = 0;
                for (r = Limit; r <= (Width - Limit) - 1; r++)
                    for (c = Limit; c <= (Height - Limit) - 1; c++)
                    {
                        if (PostHysteresis[r, c] > max)
                        {
                            max = PostHysteresis[r, c];
                        }

                        if ((PostHysteresis[r, c] < min) && (PostHysteresis[r, c] > 0))
                        {
                            min = PostHysteresis[r, c];
                        }
                    }

                GNH = new float[Width, Height];
                GNL = new float[Width, Height]; ;
                EdgePoints = new int[Width, Height];

                for (r = Limit; r <= (Width - Limit) - 1; r++)
                {
                    for (c = Limit; c <= (Height - Limit) - 1; c++)
                    {
                        if (PostHysteresis[r, c] >= MaxHysteresisThresh)
                        {

                            EdgePoints[r, c] = 1;
                            GNH[r, c] = 255;
                        }
                        if ((PostHysteresis[r, c] < MaxHysteresisThresh) && (PostHysteresis[r, c] >= MinHysteresisThresh))
                        {

                            EdgePoints[r, c] = 2;
                            GNL[r, c] = 255;

                        }

                    }

                }

                HysterisisThresholding(EdgePoints);

                for (i = 0; i <= (Width - 1); i++)
                    for (j = 0; j <= (Height - 1); j++)
                    {
                        EdgeMap[i, j] = EdgeMap[i, j] * 255;
                    }

                return;

            }

            private void HysterisisThresholding(int[,] Edges)
            {

                int i, j;
                int Limit = KernelSize / 2;


                for (i = Limit; i <= (Width - 1) - Limit; i++)
                    for (j = Limit; j <= (Height - 1) - Limit; j++)
                    {
                        if (Edges[i, j] == 1)
                        {
                            EdgeMap[i, j] = 1;

                        }

                    }

                for (i = Limit; i <= (Width - 1) - Limit; i++)
                {
                    for (j = Limit; j <= (Height - 1) - Limit; j++)
                    {
                        if (Edges[i, j] == 1)
                        {
                            EdgeMap[i, j] = 1;
                            Travers(i, j);
                            VisitedMap[i, j] = 1;
                        }
                    }
                }




                return;
            }

            private void Travers(int X, int Y)
            {


                if (VisitedMap[X, Y] == 1)
                {
                    return;
                }

                //1
                if (EdgePoints[X + 1, Y] == 2)
                {
                    EdgeMap[X + 1, Y] = 1;
                    VisitedMap[X + 1, Y] = 1;
                    Travers(X + 1, Y);
                    return;
                }
                //2
                if (EdgePoints[X + 1, Y - 1] == 2)
                {
                    EdgeMap[X + 1, Y - 1] = 1;
                    VisitedMap[X + 1, Y - 1] = 1;
                    Travers(X + 1, Y - 1);
                    return;
                }

                //3

                if (EdgePoints[X, Y - 1] == 2)
                {
                    EdgeMap[X, Y - 1] = 1;
                    VisitedMap[X, Y - 1] = 1;
                    Travers(X, Y - 1);
                    return;
                }

                //4

                if (EdgePoints[X - 1, Y - 1] == 2)
                {
                    EdgeMap[X - 1, Y - 1] = 1;
                    VisitedMap[X - 1, Y - 1] = 1;
                    Travers(X - 1, Y - 1);
                    return;
                }
                //5
                if (EdgePoints[X - 1, Y] == 2)
                {
                    EdgeMap[X - 1, Y] = 1;
                    VisitedMap[X - 1, Y] = 1;
                    Travers(X - 1, Y);
                    return;
                }
                //6
                if (EdgePoints[X - 1, Y + 1] == 2)
                {
                    EdgeMap[X - 1, Y + 1] = 1;
                    VisitedMap[X - 1, Y + 1] = 1;
                    Travers(X - 1, Y + 1);
                    return;
                }
                //7
                if (EdgePoints[X, Y + 1] == 2)
                {
                    EdgeMap[X, Y + 1] = 1;
                    VisitedMap[X, Y + 1] = 1;
                    Travers(X, Y + 1);
                    return;
                }
                //8

                if (EdgePoints[X + 1, Y + 1] == 2)
                {
                    EdgeMap[X + 1, Y + 1] = 1;
                    VisitedMap[X + 1, Y + 1] = 1;
                    Travers(X + 1, Y + 1);
                    return;
                }


                //VisitedMap[X, Y] = 1;
                return;
            }
        }


    }
