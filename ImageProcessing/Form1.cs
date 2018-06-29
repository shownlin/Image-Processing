using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //建立 OpenFileDialog 元件
            OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
            OpenFileDialog1.InitialDirectory = "D:\\";

            //顯示出對話框
            if (OpenFileDialog1 .ShowDialog ()== DialogResult .OK )
            {
                pictureBox1.ImageLocation = OpenFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Jpeg Image |*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png";
            dlg.Title = "Save an Image File";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlg.FileName != "")
            {
                switch (dlg.FilterIndex)
                {
                    case 1:
                        pictureBox2.Image.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case 2:
                        pictureBox2.Image.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 3:
                        pictureBox2.Image.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case 4:
                        pictureBox2.Image.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                        
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            Bitmap old = new Bitmap(pictureBox1.Image);
            Bitmap newone = old;

            switch ((String)comboBox1.SelectedValue)
            {
                case "Gray":
                    newone = Gray(old);
                    break;
                case "Invert":
                    newone = Invert(old);
                    break;
                case "pl":
                    newone = pl(old);
                    break;
                case "log":
                    newone = log(old);
                    break;
                case "Histogram":
                    newone = HistogramEqualization(old);
                    break;
                case "meanFilter":
                    newone = Filter.mean_filter(old);
                    break;
                case "SobelFilter":
                    if (checkBox1.Checked == true)
                        newone = Filter.sobelOperator_filter(Gray(old));
                    else
                        newone = Filter.sobelOperator_filter(old);
                    break;
                case "OTSU":
                    newone = Filter.Otsu(old);
                    break;
            }
            pictureBox2.Image = newone;
        }

        public class cboDataList
        {
            public string cbo_Name { get; set; }
            public string cbo_Value { get; set; }
        }


        public  Bitmap Gray(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite,PixelFormat.Format24bppRgb);
            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;
            //算出畸零地的字節數
            int srcOffset = srcBmData.Stride - srcBmData.Width * 3; 

            Bitmap dstBitmap = createImage(wide, height);
            BitmapData dstBmData = dstBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            System.IntPtr dstScan = dstBmData.Scan0;
            int dstOffset = dstBmData.Stride - dstBmData.Width * 3;

            unsafe //啟動不安全代碼
        {
                byte* srcP = (byte*)srcScan.ToPointer();
                byte* dstP = (byte*)dstScan.ToPointer();

                double gray = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        gray = srcP[2] * 0.299 + srcP[1] * 0.587 + srcP[0] * 0.114;
                        //根據Y=0.299*R+0.114*G+0.587B,Y為亮度
                        dstP[0] = dstP[1] = dstP[2] = (byte)gray;
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

        public  Bitmap Invert(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite,PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;

            unsafe //啟動不安全代碼
            {
                byte* srcP = (byte*)srcScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {

                        for (int z = 0; z < 3; z++) { 
                            srcP[0] = (byte)(255 - srcP[0]);
                            srcP++;
                        }
                    }
                }
            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);

            return srcBitmap;
        }

        public Bitmap pl(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite,PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;

            unsafe //啟動不安全代碼
            {
                byte* srcP = (byte*)srcScan.ToPointer();

                double c = Convert.ToDouble(textBox1.Text);
                double gamma = Convert.ToDouble(textBox2.Text);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {

                        srcP[0] = (byte)((c * Math.Pow(srcP[0] / 255.0, gamma))*255.0);

                        srcP[1] = srcP[2] = srcP[0];

                        srcP += 3; //位圖結構中RGB按BGR的順序排列，offset = 3
                    }
                }
            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);

            return srcBitmap;
        }

        public Bitmap log(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite,
            PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;

            unsafe //啟動不安全代碼
            {
                byte* srcP = (byte*)srcScan.ToPointer();

                double c = Convert.ToDouble(textBox1.Text);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {

                        srcP[0] = (byte)((c * Math.Log(1 + srcP[0] / 255.0, 10)) * 255.0);
                        srcP++;
                        srcP[0] = (byte)((c * Math.Log(1 + srcP[0] / 255.0, 10)) * 255.0);
                        srcP++;
                        srcP[0] = (byte)((c * Math.Log(1 + srcP[0] / 255.0, 10)) * 255.0);
                        srcP++;
                    }
                }
            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);

            return srcBitmap;
        }

        public Bitmap HistogramEqualization(Bitmap srcBitmap)
        {
            int wide = srcBitmap.Width;
            int height = srcBitmap.Height;
            double  v = wide * height;
            Rectangle rect = new Rectangle(0, 0, wide, height);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData srcBmData = srcBitmap.LockBits(rect, ImageLockMode.ReadWrite,PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            System.IntPtr srcScan = srcBmData.Scan0;
            //算出畸零地的字節數
            int srcOffset = srcBmData.Stride - srcBmData.Width * 3;

            unsafe //啟動不安全代碼
            {
                byte* srcP = (byte*)srcScan.ToPointer();

                double[] count = new double[256]; //累計每個像素同顏色的數量
                double[] p = new double[256]; //計算每個顏色在此圖中出現的機率（除以總像素）
                double[] AccumulateP = new double[256];//這些機率值以"目前項=前面幾項累加"的方式得到新的機率值
                double[] gray = new double[256];


                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        count[srcP[2]] += 1 ;
                        srcP += 3; //以R平面的灰度值來取代 G及B 平面（針對灰階）

                    }
                    srcP += srcOffset;
                }

                for (int i = 0; i < 256; i++)
                    p[i] = count[i]; //計算PDF（Probability Density Function）

                for (int m = 0; m < 256; m++) //計算CDF（Cumulative Distribution Function）
                {
                    for (int n = 0; n <= m; n++)
                    {
                        AccumulateP[m] += p[n];
                    }

                }

                for (int i = 0; i < 256; i++) //normalize
                {
                    gray[i] = AccumulateP[i] / v * 255.0;
                }

                srcP = (byte*)srcScan.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        srcP[0] = srcP[1] = srcP[2] = (byte)gray[srcP[2]];                        
                        srcP += 3; //位圖結構中RGB按BGR的順序排列，offset = 3

                    }
                    srcP += srcOffset;
                }


            }
            // 解鎖位圖
            srcBitmap.UnlockBits(srcBmData);

            return srcBitmap;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // e.KeyChar == (Char)48 ~ 57 -----> 0~9
            // e.KeyChar == (Char)8 -----------> Backpace
            // e.KeyChar == (Char)13-----------> Enter
            if (e.KeyChar == (Char)48 || e.KeyChar == (Char)49 ||
               e.KeyChar == (Char)50 || e.KeyChar == (Char)51 ||
               e.KeyChar == (Char)52 || e.KeyChar == (Char)53 ||
               e.KeyChar == (Char)54 || e.KeyChar == (Char)55 ||
               e.KeyChar == (Char)56 || e.KeyChar == (Char)57 ||
               e.KeyChar == (Char)13 || e.KeyChar == (Char)8)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<cboDataList> lis_DataList = new List<cboDataList>()
            {
                new cboDataList
                {
                    cbo_Name = "灰階化",
                    cbo_Value = "Gray"
                },
                new cboDataList
                {
                    cbo_Name = "負片效果",
                    cbo_Value = "Invert"
                },
                 new cboDataList
                {
                    cbo_Name = "Power-Law",
                    cbo_Value = "pl"
                },
                new cboDataList
                {
                    cbo_Name = "對數轉換",
                    cbo_Value = "log"
                },
                new cboDataList
                {
                    cbo_Name = "直方圖等化",
                    cbo_Value = "Histogram"
                },
                new cboDataList
                {
                    cbo_Name = "均值濾波器",
                    cbo_Value = "meanFilter"
                },
                new cboDataList
                {
                    cbo_Name = "Sobel濾波器 ",
                    cbo_Value = "SobelFilter"
                },
                new cboDataList
                {
                    cbo_Name = "歐蘇法",
                    cbo_Value = "OTSU"
                }
            };

            comboBox1.DataSource = lis_DataList;
            comboBox1.DisplayMember = "cbo_Name";
            comboBox1.ValueMember = "cbo_Value";

            textBox1.ShortcutsEnabled = false;  // 不啟用快速鍵

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ("log".Equals(comboBox1.SelectedValue))
            {
                label1.Visible = textBox1.Visible = true;
                label2.Visible = textBox2.Visible = false;
                textBox1.Text = "50";
                checkBox1.Visible = false;
            }

            else if ("pl".Equals(comboBox1.SelectedValue))
            {
                label1.Visible = textBox1.Visible = true;
                textBox1.Text = "1";
                label2.Visible = textBox2.Visible = true;
                textBox2.Text = "0.4";
                checkBox1.Visible = false;
            }

            else if ("SobelFilter".Equals(comboBox1.SelectedValue))
            {
                label1.Visible = textBox1.Visible = label2.Visible = textBox2.Visible = false;
                textBox1.Text = textBox2.Text = "";
                checkBox1.Visible = true;
            }

            else
            {
                label1.Visible = textBox1.Visible = label2.Visible = textBox2.Visible = false;
                textBox1.Text = textBox2.Text = "";
                checkBox1.Visible = false;
            }
        }

        private Bitmap createImage(int wide, int height)
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
