﻿using System.Drawing;
using System.Drawing.Imaging;
using Greenshot.Gfx;
using Greenshot.Gfx.Experimental;
using Greenshot.Tests.Implementation;
using Xunit;

namespace Greenshot.Tests
{
    /// <summary>
    /// This tests if the new blur works
    /// </summary>
    public class BlurTests
    {
        [Theory]
        [InlineData(PixelFormat.Format24bppRgb)]
        [InlineData(PixelFormat.Format32bppRgb)]
        [InlineData(PixelFormat.Format32bppArgb)]
        public void Test_Blur(PixelFormat pixelFormat)
        {
            using (var bitmapNew = BitmapFactory.CreateEmpty(400, 400, pixelFormat, Color.White))
            using (var bitmapOld = BitmapFactory.CreateEmpty(400, 400, pixelFormat, Color.White))
            {
                using (var graphics = Graphics.FromImage(bitmapNew))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                    bitmapNew.ApplyBoxBlur(10);
                }
                using (var graphics = Graphics.FromImage(bitmapOld))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                    bitmapOld.ApplyOldBoxBlur(10);
                }
                bitmapOld.Save(@"old.png", ImageFormat.Png);
                bitmapNew.Save(@"new.png", ImageFormat.Png);

                Assert.True(bitmapOld.IsEqualTo(bitmapNew), "New blur doesn't compare to old.");
            }
        }

        [Theory]
        [InlineData(PixelFormat.Format24bppRgb)]
        [InlineData(PixelFormat.Format32bppRgb)]
        [InlineData(PixelFormat.Format32bppArgb)]
        public void Test_Blur_Span(PixelFormat pixelFormat)
        {
            using (var bitmapNew = BitmapFactory.CreateEmpty(400, 400, pixelFormat, Color.White))
            using (var bitmapOld = BitmapFactory.CreateEmpty(400, 400, pixelFormat, Color.White))
            {
                using (var graphics = Graphics.FromImage(bitmapNew))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                    bitmapNew.ApplyBoxBlurSpan(10);
                }
                using (var graphics = Graphics.FromImage(bitmapOld))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                    bitmapOld.ApplyOldBoxBlur(10);
                }
                bitmapOld.Save(@"old.png", ImageFormat.Png);
                bitmapNew.Save(@"new.png", ImageFormat.Png);

                Assert.True(bitmapOld.IsEqualTo(bitmapNew), "New blur doesn't compare to old.");
            }
        }
    }
}
