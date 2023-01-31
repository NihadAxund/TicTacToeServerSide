using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TicTacToeServerSide.Help
{
    public class UserLogin
    {
        public byte[] ImageBrush_ { get; set; }
        public string UserName { get; set; }
        public UserLogin()
        {

        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public UserLogin(BitmapImage BI, string userName)
        {
            MemoryStream test = new MemoryStream();
            Bitmap bmp = BitmapImage2Bitmap(BI);
            //using (Graphics g = Graphics.FromImage(BitmapImage2Bitmap(BI)))
            //{
            //    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            //    bmp.Save(test, ImageFormat.Jpeg);

            //    //bmp.Save(path, ImageFormat.Png);
            //}
            //ImageBrush_ = IB;
            bmp.Save(test, ImageFormat.Jpeg);
            ImageBrush_ = test.ToArray();
            UserName = userName;
        }
    }
}
