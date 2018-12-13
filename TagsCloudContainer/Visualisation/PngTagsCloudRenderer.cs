﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TagsCloudContainer.TagsClouds;

namespace TagsCloudContainer.Visualisation
{
    public class PngTagsCloudRenderer : ITagsCloudRenderer
    {
        private readonly Size boundary = new Size(100, 100);
        private readonly FontFamily fontFamily;
        private readonly Color textColor;
        private readonly IColorManager colorManager;
        private readonly Size pictureSize;
        private readonly Dictionary<TagsCloudWord, Color> wordsColors;

        public PngTagsCloudRenderer(ImageSettings imageSettings)
        {
            pictureSize = imageSettings.ImageSize;
            textColor = imageSettings.TextColor;
            fontFamily = imageSettings.FontFamily;
        }

        public void RenderIntoFile(string filePath, ITagsCloud tagsCloud, IColorManager colorManager,
            bool autosize = false)
        {
            var words = tagsCloud.AddedWords.Select(x => x.Word).ToList();
            if (autosize)
            {
                RenderIntoFileAutosize(filePath, tagsCloud, colorManager);
                return;
            }

            var wordsColors = colorManager.GetWordsColors(tagsCloud.AddedWords.ToList());


            var btm = new Bitmap(pictureSize.Width, pictureSize.Height);
            using (Graphics obj = Graphics.FromImage(btm))
            {
                foreach (var tagsCloudWord in tagsCloud.AddedWords)
                {
                    DrawWord(obj, tagsCloudWord, wordsColors[tagsCloudWord]);
                }

                btm.Save(filePath);
            }
        }

        private void DrawWord(Graphics graphics, TagsCloudWord tagsCloudWord, Color color)
        {
            var rectangle = tagsCloudWord.Rectangle;
            var fontSize = rectangle.Height;
            graphics.DrawString(tagsCloudWord.Word, new Font(fontFamily, fontSize),
                new SolidBrush(color),
                new PointF(rectangle.X - fontSize / 4, rectangle.Y - fontSize / 4));
        }

        public void RenderIntoFileAutosize(string filePath, ITagsCloud tagsCloud, IColorManager colorManager)
        {
            var words = tagsCloud.AddedWords.Select(x => x.Word).ToList();
            var shiftedRectangles =
                ShiftRectanglesToMainQuarter(tagsCloud.AddedWords.Select(x => x.Rectangle).ToList());
            var tagsCloudWords = words.Zip(shiftedRectangles, (a, b) => (new TagsCloudWord(a, b))).ToList();
            var tagsCloudToDraw = new TagsCloud(tagsCloudWords);
            var wordsColors = colorManager.GetWordsColors(tagsCloudToDraw.AddedWords.ToList());

            var pictureSize = GetPictureSize(tagsCloudToDraw);

            var btm = new Bitmap(pictureSize.Width, pictureSize.Height);
            using (Graphics obj = Graphics.FromImage(btm))
            {
                foreach (var tagsCloudWord in tagsCloudToDraw.AddedWords)
                {
                    DrawWord(obj, tagsCloudWord, wordsColors[tagsCloudWord]);
                }

                btm.Save(filePath);
            }
        }


        private Size GetPictureSize(ITagsCloud tagsCloud)
        {
            var rectangles = tagsCloud.AddedWords.Select(x => x.Rectangle);
            var maxX = rectangles.Max(x => x.Right);
            var minX = rectangles.Min(x => x.X);
            var maxY = rectangles.Max(x => x.Top);
            var minY = rectangles.Min(x => x.Bottom);
            if (minY < 0 || minX < 0)
            {
                throw new ArgumentException("Rectangles must have positive coordinates");
            }

            return new Size(maxX - minX + Math.Abs(minX * 2),
                maxY - minY + Math.Abs(minY * 2));
        }

        private List<Rectangle> ShiftRectanglesToMainQuarter(List<Rectangle> rectangles)
        {
            var minX = rectangles.Min(x => x.X);
            var minY = rectangles.Min(x => x.Bottom);
            var shiftX = 0;
            var shiftY = 0;
            if (minX < 0)
                shiftX = minX * -1 + boundary.Width;
            if (minY < 0)
                shiftY = minY * -1 + boundary.Height;

            return rectangles.Select(x => new Rectangle(x.X + shiftX, x.Y + shiftY, x.Width, x.Height)).ToList();
        }
    }
}