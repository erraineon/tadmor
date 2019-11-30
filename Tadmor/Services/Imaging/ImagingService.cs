using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImageMagick;
using MoreLinq;

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
            IEnumerable<RngImage> rngAvatars,
            IList<string> options)
        {
            if (options.Count % 2 != 0) throw new Exception("need an even number of options");
            if (options.Count > 16) throw new Exception("please no");

            var cellSize = new Size(500, 300);
            var cellPadding = new Size(30, 20);
            const int panelHeight = 210;
            const int textHeight = 50;
            var axisLength = options.Count / 2;
            var canvasSize = cellSize * axisLength;
            using var canvas = new MagickImage(MagickColors.Black, canvasSize.Width, canvasSize.Height);

            // create an n*n labeled matrix, then join each cell with its avatars based on seeded rng
            var cells = options.Take(axisLength)
                .Cartesian(options.Skip(axisLength), (s1, s2) => s1 == s2 ? $"true {s1}" : $"{s1} {s2}")
                .Batch(axisLength)
                .SelectMany((col, x) => col.Select((cell, y) => (text: cell.ToUpper(), x, y)))
                .GroupJoin(rngAvatars.Select(a => a.Extend(options)),
                    cell => (cell.x, cell.y),
                    avatar => (avatar.Random.Next(axisLength), avatar.Random.Next(axisLength)),
                    (cell, avatars) => (
                        cell.text,
                        new Point(cell.x * cellSize.Width, cell.y * cellSize.Height),
                        avatars.Select(t => CropCircle(t.ImageData)).ToList()));

            foreach (var (text, cellPos, avatars) in cells)
            {
                var cellRect = new Rectangle(cellPos, cellSize);
                cellRect.Inflate(cellPadding * -1);

                var panelRect = new Rectangle(cellRect.Location, new Size(cellRect.Width, panelHeight));
                var drawables = new Drawables()
                    .Rectangle(panelRect.Left, panelRect.Top, panelRect.Right, panelRect.Bottom);
                if (avatars.Any())
                {
                    var stackedAvatars = StackHorizontally(avatars, panelRect.Size);
                    var averageColor = GetAverageColor(stackedAvatars);
                    drawables
                        .FillColor(averageColor)
                        .Composite(stackedAvatars, panelRect, Gravity.Center);
                }
                else
                {
                    drawables.FillColor(MagickColors.LightGray);
                }

                var textRect = new Rectangle(cellRect.Left, cellRect.Bottom - textHeight, cellRect.Width, textHeight);
                drawables
                    .TextFill(text, textRect, TimesNewRomanFont, TextAlignment.Center, MagickColors.White)
                    .Draw(canvas);
            }

            return canvas.ToByteArray(MagickFormat.Png);
        }

        public byte[] Rank(
            IEnumerable<RngImage> rngAvatars,
            IList<string> tiers)
        {
            if (!tiers.Any()) throw new Exception("need at least one tier");

            const int rowHeight = 140;
            var headerSize = new Size(250, rowHeight);
            var valueSize = new Size(1000, rowHeight);
            var valuesPadding = new Size(6, 6);
            var canvasSize = new Size(headerSize.Width + valueSize.Width, rowHeight * tiers.Count);
            using var canvas = new MagickImage(MagickColors.White, canvasSize.Width, canvasSize.Height);

            var avatarsByRow = rngAvatars
                .Select(avatar => avatar.Extend(tiers))
                .ToLookup(avatar => avatar.Random.Next(tiers.Count), tuple => CropCircle(tuple.ImageData));

            var drawables = new Drawables()
                .Line(headerSize.Width, 0, headerSize.Width, canvasSize.Height);

            for (var i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                var rowY = rowHeight * i;
                var textRect = new Rectangle(new Point(0, rowY), headerSize);
                drawables.Caption(tier, textRect, ArialFont, Gravity.Center, 40);
                if (rowY > 0) drawables.Line(0, rowY, canvasSize.Width, rowY);

                var avatars = avatarsByRow[i].ToList();
                if (avatars.Any())
                {
                    var avatarsRect = new Rectangle(headerSize.Width, rowY, valueSize.Width, valueSize.Height);
                    avatarsRect.Inflate(valuesPadding * -1);
                    var stackedAvatars = StackHorizontally(avatars, avatarsRect.Size);
                    drawables.Composite(stackedAvatars, avatarsRect, Gravity.West);
                }
            }

            drawables.Draw(canvas);
            return canvas.ToByteArray(MagickFormat.Png);
        }

        public byte[] UpDownGif(string text, byte[]? avatarData, string baseFilename)
        {
            //credits to https://twitter.com/reedjeyy for base images, font and constants
            const float fadeDuration = 11;
            const int fadeStartIndex = 65;
            const int textLeftMargin = 10;
            const int textRightMargin = 255;
            const int textVerticalMargin = 100;
            const int avatarRightMargin = 8;

            var frames = new MagickImageCollection(ResourcesPath + baseFilename);
            frames.Coalesce();
            var canvas = frames.First();

            using var customImage = new MagickImage(MagickColors.Transparent, canvas.Width, canvas.Height);
            var textPos = new Point(textLeftMargin, textVerticalMargin);
            var textSize = new Size(canvas.Width - textRightMargin, canvas.Height - textVerticalMargin * 2);
            var textRect = new Rectangle(textPos, textSize);
            var drawables = new Drawables().TextFill(text, textRect, GothamRoundedLightFont, TextAlignment.Right, MagickColors.White);
            if (avatarData != null)
            {
                var avatar = CropCircle(avatarData);
                var avatarRect = new Rectangle(0, 0, canvas.Width - avatarRightMargin, canvas.Height);
                drawables.Composite(avatar, avatarRect, Gravity.East);
            }
            drawables.Draw(customImage);

            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                var opacity = 1 - Math.Clamp(1 / fadeDuration * (i - fadeStartIndex), 0, 1);
                customImage.SetOpacity(opacity);
                frame.Composite(customImage, CompositeOperator.Over);
            }

            return frames.ToByteArray(MagickFormat.Gif);
        }

        public byte[] Ok(string text, byte[] avatarData)
        {
            var avatarSize = new Size(70, 74);
            var avatarPosition = new Point(4, 4);
            var textPos = new Point(81, 9);

            using var canvas = new MagickImage(ResourcesPath + "angry.png");
            using var avatar = new MagickImage(avatarData);
            avatar.Resize(new MagickGeometry(avatarSize.Width, avatarSize.Height) {IgnoreAspectRatio = true});
            avatar.Quantize(new QuantizeSettings
            {
                Colors = 10,
                ColorSpace = ColorSpace.RGB,
                DitherMethod = DitherMethod.Riemersma
            });
            var textRect = new Rectangle(textPos, new Size(canvas.Width - textPos.X, canvas.Height - textPos.Y));
            new Drawables()
                .Composite(avatarPosition.X, avatarPosition.Y, avatar)
                .Caption(text, textRect, MsSansSerifFont, Gravity.Northwest, 11)
                .Draw(canvas);
            canvas.Scale(new Percentage(300));
            return canvas.ToByteArray(MagickFormat.Png);
        }

        public byte[] Text(string name, string text)
        {
            const int textRightMargin = 20;
            var namePosition = new Point(19, 252);
            var textPosition = new Point(19, 269);
            var textColor = new MagickColor(4, 4, 4);
            var nameFont = HelveticaNeueMediumFont;
            var textFont = HelveticaNeueFont;

            using var canvas = new MagickImage(ResourcesPath + "text1.png");
            var textSize = new Size(canvas.Width - namePosition.X - textRightMargin, canvas.Height);
            new Drawables()
                .Label(name, namePosition, nameFont, Gravity.Northwest, 14, textColor: textColor)
                .Caption(text, new Rectangle(textPosition, textSize), textFont, Gravity.Northwest, 14.75, textColor)
                .Draw(canvas);
            canvas.AdaptiveResize(new MagickGeometry(new Percentage(300), new Percentage(300)));
            return canvas.ToByteArray(MagickFormat.Png);
        }

        public byte[] Quadrant(IEnumerable<RngImage> rngImages, string opt1, string opt2, string opt3, string opt4)
        {
            const int s = 1280;
            const int margin = s / 25;
            const int textMargin = s / 100;
            const int median = s / 2;
            const int fontSize = 35;

            var parametersSeed = $"{opt1}{opt2}{opt3}{opt4}".ToLower();
            var avatarsAndPositions = rngImages
                .Select(i => i.Extend(parametersSeed))
                .Select(i => (avatar: CropCircle(i.ImageData), pos: new Point(i.Random.Next(s), i.Random.Next(s))));
            using var canvas = new MagickImage(MagickColors.White, s, s);
            var drawables = new Drawables()
                .StrokeWidth(5)
                .Line(median, margin, median, s - margin)
                .Line(margin, median, s - margin, median);

            foreach (var (image, pos) in avatarsAndPositions)
            {
                drawables
                    .Composite(pos.X - image.Width / 2, pos.Y - image.Height / 2, CompositeOperator.Over, image);
            }
            var opt1And2Rect = new Rectangle(0, 0, s, s);
            opt1And2Rect.Inflate(-textMargin, -textMargin);
            drawables
                .Label(opt1, new Point(median, textMargin), ArialFont, Gravity.North, fontSize, backgroundColor: MagickColors.White)
                .Label(opt2, new Point(median, s - textMargin), ArialFont, Gravity.South, fontSize, backgroundColor: MagickColors.White)
                .Label(opt3, new Point(textMargin, median), ArialFont, Gravity.West, fontSize, backgroundColor: MagickColors.White)
                .Label(opt4, new Point(s - textMargin, median), ArialFont, Gravity.East, fontSize, backgroundColor: MagickColors.White)
                .Draw(canvas);
            return canvas.ToByteArray(MagickFormat.Png);
        }

        public byte[] Poly(List<RngImage> rngImages, IList<string> options)
        {
            static PointF RandomPointInTriangle(Random rng, SizeF a, SizeF b, SizeF c)
            {
                var r1 = (float)rng.NextDouble();
                var r2 = (float)rng.NextDouble();
                var size = (1 - (float)Math.Sqrt(r1)) * a +
                             (float)Math.Sqrt(r1) * (1 - r2) * b +
                             r2 * (float)Math.Sqrt(r1) * c;
                return size.ToPointF();
            }

            if (options.Count < 3) throw new Exception("need at least three options");
            const int s = 1280;
            const int median = s / 2;
            const float polyRadius = s * 0.45F;
            const int margin = 10;
            var polyCenter = new SizeF(median, median);

            //computed variables
            var x = options
                .Select((option, i) =>
                {
                    var angle = (float) (Math.PI * 2) / options.Count * i;
                    var unitCirclePoint = new SizeF(-MathF.Sin(angle), -MathF.Cos(angle));
                    var vertex = (polyCenter + unitCirclePoint * polyRadius).ToPointF();
                    var textPos = (polyCenter + unitCirclePoint * median).ToPointF();
                    return (option, vertex, textPos: new Point((int) textPos.X, (int) textPos.Y));
                })
                .ToList();
            var verticalOffset = (s - x.Max(t => t.vertex.Y) - x.Min(t => t.vertex.Y)) / 2;

            using var canvas = new MagickImage(MagickColors.White, s, s);
            var drawables = new Drawables()
                .Translation(0, verticalOffset)
                .StrokeWidth(5)
                .FillColor(MagickColors.Transparent)
                .StrokeColor(MagickColors.Black)
                .Polygon(x.Select(p => new PointD(p.vertex.X, p.vertex.Y)));
            var textBounds = canvas.GetRectangle();
            textBounds.Inflate(-margin, -margin);
            foreach (var (option, _, textPos) in x)
            {
                drawables.Label(option, textPos, ArialFont, Gravity.Center, 28, textBounds);
            }

            //avatars
            foreach (var rngImage in rngImages)
            {
                rngImage.Extend(options);
                var avatar = CropCircle(rngImage.ImageData);
                var vIndex = rngImage.Random.Next(x.Count);
                var a = polyCenter;
                var b = new SizeF(x[vIndex].vertex);
                var c = new SizeF(x[(vIndex + 1) % x.Count].vertex);
                var avatarPos = RandomPointInTriangle(rngImage.Random, a, b, c);
                drawables.Composite(avatar, new Point((int) avatarPos.X, (int) avatarPos.Y), Gravity.Center);
            }

            drawables.Draw(canvas);
            return canvas.ToByteArray(MagickFormat.Png);
        }

        private static MagickImage CropCircle(byte[]? imageData)
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

        private static MagickColor GetAverageColor(IMagickImage image)
        {
            using var clonedImage = image.Clone();
            clonedImage.Resize(1, 1);
            var singlePixel = clonedImage.GetPixels().Single();
            var averageColor = singlePixel.ToColor();
            return averageColor;
        }

        private static MagickImage StackHorizontally(IReadOnlyCollection<MagickImage> images, Size maxSize)
        {
            var panel = new MagickImage(MagickColors.Transparent,
                images.Sum(a => a.Width),
                images.Max(a => a.Height));

            var avatarX = 0;
            foreach (var avatar in images)
            {
                panel.Composite(avatar, avatarX, 0, CompositeOperator.Over);
                avatarX += avatar.Width;
            }

            if (panel.Width > maxSize.Width || panel.Height > maxSize.Height)
            {
                panel.Resize(maxSize.Width, maxSize.Height);
            }

            return panel;
        }
    }
}