using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{ 


    public class Filter
    {
	    public Filter()
	    {
        }

        public static Bitmap mean_filter(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;
            //算出畸零地的字節數
            int srcOffset = srcBmData.Stride - srcBmData.Width * 3;

            Bitmap dstBitmap = createImage(wide, height);
            BitmapData dstBmData = dstBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            System.IntPtr dstScan = dstBmData.Scan0;
            int dstOffset = dstBmData.Stride - dstBmData.Width * 3;

            int Stride = dstBmData.Stride;

            int[,] mask = new int[9,3];

            unsafe
            {
                byte* srcP = (byte*)srcScan.ToPointer();
                byte* dstP = (byte*)dstScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        if ((y != 0) & (y != height  - 1) & (x != 0) & (x != wide - 1))     //不考慮最外層像素      

                        {
                            for (int i = 0; i < 3; i++) //遮罩左上角
                                mask[0, i] = srcP[i - Stride - 3];
                            for (int i = 0; i < 3; i++) //遮罩中間上
                                mask[1, i] = srcP[i - Stride];
                            for (int i = 0; i < 3; i++) //遮罩右上角
                                mask[2, i] = srcP[i - Stride + 3];
                            for (int i = 0; i < 3; i++) //遮罩左方
                                mask[3, i] = srcP[i - 3];
                            for (int i = 0; i < 3; i++) //遮罩中間
                                mask[4, i] = srcP[i];
                            for (int i = 0; i < 3; i++) //遮罩右方
                                mask[5, i] = srcP[i + 3];
                            for (int i = 0; i < 3; i++) //遮罩左下角
                                mask[6, i] = srcP[i + Stride - 3];
                            for (int i = 0; i < 3; i++) //遮罩中間下
                                mask[7, i] = srcP[i + Stride];
                            for (int i = 0; i < 3; i++) //遮罩右下方
                                mask[8, i] = srcP[i + Stride + 3];

                            double averageR = 0;
                            double averageG = 0;
                            double averageB = 0;

                            for (int i = 0; i < 9; i++)
                                averageR += mask[i, 2];
                            for (int i = 0; i < 9; i++)
                                averageG += mask[i, 1];    
                            for (int i = 0; i < 9; i++)
                                averageB += mask[i, 0];

                            //都除以9得出平均值
                            averageR /= 9.0;
                            averageG /= 9.0;
                            averageB /= 9.0;

                            dstP[0] = (byte)averageB;
                            dstP[1] = (byte)averageG;
                            dstP[2] = (byte)averageR;
                        }
                        else
                        {
                            dstP[0] = srcP[0];
                            dstP[1] = srcP[1];
                            dstP[2] = srcP[2];
                        }

                        srcP += 3; //位圖結構中RGB按BGR的順序排列，offset = 3
                        dstP += 3;
                    }
                    srcP += srcOffset;
                    dstP += dstOffset;
                }
            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);
            dstBitmap.UnlockBits(dstBmData);

            return dstBitmap;
        }

        public static Bitmap sobelOperator_filter(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;
            //算出畸零地的字節數
            int srcOffset = srcBmData.Stride - srcBmData.Width * 3;

            Bitmap dstBitmap = createImage(wide, height);
            BitmapData dstBmData = dstBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            System.IntPtr dstScan = dstBmData.Scan0;
            int dstOffset = dstBmData.Stride - dstBmData.Width * 3;

            int Stride = dstBmData.Stride;

            double[,] maskX = new double[9, 3];   //儲存X方向遮罩運算結果
            double[,] maskY = new double[9, 3];   //儲存Y方向遮罩運算結果

            unsafe
            {
                byte* srcP = (byte*)srcScan.ToPointer();
                byte* dstP = (byte*)dstScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        if ((y != 0) & (y != height - 1) & (x != 0) & (x != wide - 1))     //不考慮最外層像素      

                        {

                            for (int i = 0; i < 3; i++) //遮罩左上角
                            {
                                maskX[0, i] = srcP[i - Stride - 3] * (-1);
                                maskY[0, i] = srcP[i - Stride - 3] * (-1);
                            }
                            for (int i = 0; i < 3; i++) //遮罩中間上
                            {
                                maskX[1, i] = 0;
                                maskY[1, i] = srcP[i - Stride] * (-2);
                            }

                            for (int i = 0; i < 3; i++) //遮罩右上角
                            {
                                maskX[2, i] = srcP[i - Stride + 3] * 1;
                                maskY[2, i] = srcP[i - Stride + 3] * (-1);
                            }
                            for (int i = 0; i < 3; i++) //遮罩左方
                            {
                                maskX[3, i] = srcP[i - 3] * (-2);
                                maskY[3, i] = 0;
                            }
                            for (int i = 0; i < 3; i++) //遮罩中間
                            {
                                maskX[4, i] = 0;
                                maskY[4, i] = 0;
                            }
                            for (int i = 0; i < 3; i++) //遮罩右方
                            {
                                maskX[5, i] = srcP[i + 3] * 2;
                                maskY[5, i] = 0; ;
                            }
                            for (int i = 0; i < 3; i++) //遮罩左下角
                            {
                                maskX[6, i] = srcP[i + Stride - 3] * (-1);
                                maskY[6, i] = srcP[i + Stride - 3] * 1;
                            }
                            for (int i = 0; i < 3; i++) //遮罩中間下
                            {
                                maskX[7, i] = 0;
                                maskY[7, i] = srcP[i + Stride] * 2;
                            }
                            for (int i = 0; i < 3; i++) //遮罩右下方
                            { 
                                maskX[8, i] = srcP[i + Stride + 3] * 1;
                                maskY[8, i] = srcP[i + Stride + 3] * 1;
                            }

                            double xR = 0;
                            double xG = 0;
                            double xB = 0;
                            double yR = 0;
                            double yG = 0;
                            double yB = 0;

                            for (int i = 0; i < 9; i++)
                            {
                                xR += (maskX[i, 2] / 255.0);
                                yR += (maskY[i, 2] / 255.0);
                            }
                            for (int i = 0; i < 9; i++)
                            {
                                xG += (maskX[i, 1] / 255.0);
                                yG += (maskY[i, 1] / 255.0);
                            }
                            for (int i = 0; i < 9; i++)
                            {
                                xB += (maskX[i, 0] / 255.0);
                                yB += (maskY[i, 0] / 255.0);
                            }

                            dstP[0] = (byte)(Math.Sqrt((Math.Pow(xB, 2) + Math.Pow(yB, 2))) * 255.0);
                            dstP[1] = (byte)(Math.Sqrt((Math.Pow(xG, 2) + Math.Pow(yG, 2))) * 255.0);
                            dstP[2] = (byte)(Math.Sqrt((Math.Pow(xR, 2) + Math.Pow(yR, 2))) * 255.0);

                        }
                        else
                        {
                            dstP[0] = srcP[0];
                            dstP[1] = srcP[1];
                            dstP[2] = srcP[2];
                        }

                        srcP += 3; //位圖結構中RGB按BGR的順序排列，offset = 3
                        dstP += 3;
                    }
                    srcP += srcOffset;
                    dstP += dstOffset;
                }

            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);
            dstBitmap.UnlockBits(dstBmData);

            return dstBitmap;
        }

        public static Bitmap Otsu(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;
            //算出畸零地的字節數
            int srcOffset = srcBmData.Stride - srcBmData.Width * 3;

            int RT,GT,BT;
            double minVariantR, minVariantG, minVariantB;//T為門檻值，minVariant為群內最小變異
            double wR1, wR2, wG1, wG2, wB1, wB2; //計算群1與群2的機率暫存器
            double uR1, uR2, uG1, uG2, uB1, uB2; //計算群1與群2的期望值暫存器
            double sR1, sR2, sG1, sG2, sB1, sB2; //計算群1與群2的變異量暫存器
            double sigamaR, sigamaG, sigamaB; //計算變異量總和用暫存器
            double v = wide * height; //總像素數量

            RT = GT = BT = 0;
            minVariantR = minVariantG = minVariantB = 1.7e+308;  //MAX_INT 整數類型所能表示的最大值

            unsafe
            {
                byte* srcP = (byte*)srcScan.ToPointer();

                double[] countR = new double[256]; //累計每個像素同顏色的數量
                double[] pR = new double[256]; //計算每個顏色在此圖中出現的機率（除以總像素）
                double[] countG = new double[256];
                double[] pG = new double[256];
                double[] countB = new double[256];
                double[] pB = new double[256];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {

                        for (int z = 0; z < 256; z++)
                        {
                            if (srcP[0] == z)
                                countB[z] += 1;
                            if (srcP[1] == z)
                                countG[z] += 1;
                            if (srcP[2] == z)
                                countR[z] += 1;
                        }
                        srcP += 3;
                    }
                }
                for (int i = 0; i < 256; i++)
                {
                    pB[i] = countB[i] / v; //各像素的機率值
                    pG[i] = countG[i] / v;
                    pR[i] = countR[i] / v;
                }

                for (int i = 0; i <= 255; i++)
                {
                    wR1 = wR2 = wG1 = wG2 = wB1 = wB2 = 0;

                    for (int j = 0; j <= i; j++)
                    {
                        wB1 += pB[j];
                        wG1 += pG[j];
                        wR1 += pR[j];
                    }

                    wB2 = 1 - wB1;
                    wG2 = 1 - wG1;
                    wR2 = 1 - wR1;

                    //接著計算期望值

                    uR1 = uR2 = uG1 = uG2 = uB1 = uB2 = 0;

                    for (int j = 0; j <= i; j++)
                    {
                        uB1 += pB[j] * j;
                        uG1 += pG[j] * j;
                        uR1 += pR[j] * j;
                    }
                    for (int j = i+1; j <= 255; j++)
                    {
                        uB2 += pB[j] * j;
                        uG2 += pG[j] * j;
                        uR2 += pR[j] * j;
                    }
                    uB1 /= wB1;
                    uG1 /= wG1;
                    uR1 /= wR1;
                    uB2 /= wB2;
                    uG2 /= wG2;
                    uR2 /= wR2;

                    //接著計算變異數

                    sR1 = sR2 = sG1 = sG2 = sB1 = sB2 = 0;

                    for (int j = 0; j <= i; j++)
                    {
                        sB1 += (Math.Pow((uB1 - j), 2) * pB[j]);
                        sG1 += (Math.Pow((uG1 - j), 2) * pG[j]);
                        sR1 += (Math.Pow((uR1 - j), 2) * pR[j]);
                    }

                    for (int j = i+1; j <= 255; j++)
                    {
                        sB2 += (Math.Pow((uB2 - j), 2) * pB[j]);
                        sG2 += (Math.Pow((uG2 - j), 2) * pG[j]);
                        sR2 += (Math.Pow((uR2 - j), 2) * pR[j]);
                    }

                    //接著計算群內變異總和
                    sigamaB = sB1 + sB2;
                    sigamaG = sG1 + sG2;
                    sigamaR = sR1 + sR2;

                    //判斷是否找到更好閥值（T）
                    if (sigamaB < minVariantB) //判斷B
                    {
                        BT = i;
                        minVariantB = sigamaB;
                    }
                    if (sigamaG < minVariantG) //判斷G
                    {
                        GT = i;
                        minVariantG = sigamaG;
                    }
                    if (sigamaR < minVariantR) //判斷R
                    {
                        RT = i;
                        minVariantR = sigamaR;
                    }
                }

                //進行二值化處理

                srcP = (byte*)srcScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        srcP[0] = (srcP[0] < BT) ? (byte)0 : (byte)255;
                        srcP[1] = (srcP[1] < GT) ? (byte)0 : (byte)255;
                        srcP[2] = (srcP[0] < RT) ? (byte)0 : (byte)255;

                        srcP += 3; //位圖結構中RGB按BGR的順序排列，offset = 3
                    }
                    srcP += srcOffset;
                }
            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);
            return srcBitmap;
        }


        private static Bitmap createImage(int wide, int height)
        {
            Bitmap dstBitmap = new Bitmap(wide, height);
            Rectangle rect = new Rectangle(0, 0, wide, height);
            BitmapData dstBmData = dstBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            System.IntPtr dstScan = dstBmData.Scan0;
            int offset = dstBmData.Stride - dstBmData.Width * 3; //算出畸零地的字節數

            unsafe
            {
                byte* dstP = (byte*)dstScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            *dstP = 0;
                            dstP++;
                        }
                    }

                    dstP += offset;
                }
            }
            dstBitmap.UnlockBits(dstBmData);
            return dstBitmap;
        }

    }

}