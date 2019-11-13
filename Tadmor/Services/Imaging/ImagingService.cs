using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImageMagick;
using MoreLinq;
using Tadmor.Extensions;

namespace Tadmor.Services.Imaging
{
    [SingletonService]
    public class ImagingService
    {
        private const string ResourcesPath = "Resources\\";
        private const string ArialFont = "Arial";
        private const string TimesNewRomanFont = "Times New Roman";
        private const string MsSansSerifFont = ResourcesPath + "micross.ttf";
        private const string GothamRoundedLightFont = ResourcesPath + "GothamRoundedLight.ttf";
        private const string HelveticaNeueFont = ResourcesPath + "HelveticaNeue.ttf";
        private const string HelveticaNeueMediumFont = ResourcesPath + "HelveticaNeueMedium.ttf";

        public byte[] AlignmentChart(
            IEnumerable<(Random rng, byte[] avatarData)> rngAndAvatarDatas,
            string[] options)
        {
            //constants
            const int cellW = 500;
            const int cellH = 300;
            const int horMargin = 30;
            const int verMargin = 20;
            const int textHeight = 50;
            const int textMargin = 10;
            var color = MagickColors.LightGray;

            if (options.Length % 2 != 0) throw new Exception("need an even number of options");
            if (options.Length > 16) throw new Exception("please no");
            //computed variables
            var axisLength = options.Length / 2;
            var cells = options.Take(axisLength)
                .Cartesian(options.Skip(axisLength), (s1, s2) => s1 == s2 ? $"true {s1}" : $"{s1} {s2}")
                .Batch(axisLength)
                .SelectMany((col, x) => col.Select((cell, y) => (cell: cell.ToUpper(), x, y)))
                .ToList();
            var avatarsByCell = rngAndAvatarDatas
                .Select(tuple => (rng: (options, tuple.rng.Next()).ToRandom(), tuple.avatarData))
                .ToLookup(
                    tuple => (tuple.rng.Next(axisLength), tuple.rng.Next(axisLength)),
                    tuple => CropCircle(tuple.avatarData));
            using var canvas = new MagickImage(MagickColors.Black, cellW * axisLength, cellH * axisLength);
            foreach (var (text, x, y) in cells)
            {
                //alignment cell
                var cellRect = new Rectangle(cellW * x, cellH * y, cellW, cellH - textHeight);
                cellRect.Inflate(-horMargin, -verMargin);
                var cellCenter = cellRect.Location + cellRect.Size / 2;

                //alignment text
                using var textCanvas = new MagickImage($"label:{text}", new MagickReadSettings
                {
                    FontFamily = TimesNewRomanFont,
                    Width = cellRect.Width,
                    Height = textHeight,
                    FillColor = MagickColors.White,
                    TextGravity = Gravity.Center,
                    BackgroundColor = MagickColors.Transparent
                });
                canvas.Composite(textCanvas, cellRect.Left, cellRect.Bottom + textMargin, CompositeOperator.Over);

                var avatarsForCell = avatarsByCell[(x, y)].ToList();
                if (avatarsForCell.Any())
                {
                    //draw all the avatars on one image, then resize if necessary
                    var avatarWidth = avatarsForCell.First().Width;
                    var avatars = new MagickImage(MagickColors.Transparent, avatarWidth * avatarsForCell.Count,
                        avatarWidth);

                    for (var i = 0; i < avatarsForCell.Count; i++)
                        avatars.Composite(avatarsForCell[i], i * avatarWidth, 0, CompositeOperator.Over);
                    if (avatars.Width > cellRect.Width)
                    {
                        avatars.Resize(cellRect.Width, cellRect.Width);
                    }

                    //use the average color from the avatars as cell background
                    var backgroundColorImage = avatars.Clone();
                    backgroundColorImage.Resize(1, 1);
                    var singlePixel = backgroundColorImage.GetPixels().Single();
                    canvas.Draw(new Drawables()
                        .FillColor(singlePixel.ToColor())
                        .Rectangle(cellRect.Left, cellRect.Top, cellRect.Right, cellRect.Bottom));
                    var position = cellCenter - new Size(avatars.Width, avatars.Height) / 2;
                    canvas.Composite(avatars, position.X, position.Y, CompositeOperator.Over);
                }
                else
                {
                    canvas.Draw(new Drawables()
                        .FillColor(color)
                        .Rectangle(cellRect.Left, cellRect.Top, cellRect.Right, cellRect.Bottom));
                }
            }

            return canvas.ToByteArray(MagickFormat.Png);
        }

        private MagickImage CropCircle(byte[]? imageData)
        {
            const int avatarSize = 128;
            var image = imageData != null
                ? new MagickImage(imageData)
                : new MagickImage(MagickColors.Red, avatarSize, avatarSize);
            image.Resize(new MagickGeometry(avatarSize));
            image.Alpha(AlphaOption.Set);
            var mask = image.Clone();
            mask.Distort(DistortMethod.DePolar, 0);
            mask.VirtualPixelMethod = VirtualPixelMethod.HorizontalTile;
            mask.BackgroundColor = MagickColors.None;
            mask.Distort(DistortMethod.Polar, 0);
            image.Composite(mask, CompositeOperator.DstIn);
            image.RePage();
            return image;
        }
    }
}