using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Dithering;
using SixLabors.ImageSharp.Processing.Dithering.Ordered;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;
using Tadmor.Extensions;
using Tadmor.Resources;

namespace Tadmor.Services.Imaging
{
    [SingletonService]
    public class ImagingService
    {
        private static readonly FontFamily Arial = SystemFonts.Find("Arial");
        private static readonly FontFamily TimesNewRoman = SystemFonts.Find("Times New Roman");
        private static readonly FontFamily MsSansSerif = new FontCollection().Install(Resource.Load("micross.ttf"));

        private static readonly FontFamily GothamRoundedLight =
            new FontCollection().Install(Resource.Load("GothamRoundedLight.ttf"));

        private static readonly FontFamily HelveticaNeue =
            new FontCollection().Install(Resource.Load("HelveticaNeue.ttf"));

        private static readonly FontFamily HelveticaNeueMedium =
            new FontCollection().Install(Resource.Load("HelveticaNeueMedium.ttf"));

        public MemoryStream AlignmentChart(IEnumerable<(Random rng, byte[] avatarData)> rngAndAvatarDatas,
            string[] options)
        {
            //constants
            const int cellW = 500;
            const int cellH = 300;
            const int horMargin = 30;
            const int verMargin = 20;
            const int textHeight = 80;
            const int textMargin = 10;
            var color = Rgba32.LightGray;

            if (options.Length % 2 != 0) throw new Exception("need an even number of options");
            if (options.Length > 16) throw new Exception("please no");
            //computed variables
            var axisLength = options.Length / 2;
            var cells = options.Take(axisLength)
                .Cartesian(options.Skip(axisLength), (s1, s2) => s1 == s2 ? $"true {s1}" : $"{s1} {s2}")
                .Batch(axisLength)
                .SelectMany((col, x) => col.Select((cell, y) => (cell: cell.ToUpper(), x, y)))
                .ToList();
            var alignmentString = string.Concat(cells.Select(t => t.cell));
            var rows = cells.Max(t => t.y) + 1;
            var cols = cells.Max(t => t.x) + 1;
            var avatarsByCell = rngAndAvatarDatas
                .Select(tuple => (rng: (alignmentString, tuple.rng.Next()).ToRandom(), tuple.avatarData))
                .ToLookup(
                    tuple => (tuple.rng.Next(cols), tuple.rng.Next(rows)),
                    tuple => CropCircle(tuple.avatarData));
            var font = TimesNewRoman.CreateFont(40);
            var textOptions = new TextGraphicsOptions(true) {HorizontalAlignment = HorizontalAlignment.Center};
            var output = new MemoryStream();
            using (var canvas = new Image<Rgba32>(cellW * cols, cellH * rows))
            {
                canvas.Mutate(c =>
                {
                    //background
                    c.Fill(Rgba32.Black);

                    foreach (var (text, x, y) in cells)
                    {
                        //alignment cell
                        var cellRect = new RectangleF(cellW * x, cellH * y, cellW, cellH - textHeight);
                        cellRect.Inflate(-horMargin, -verMargin);
                        var cellCenter = cellRect.Location + cellRect.Size / 2;

                        //alignment text
                        var textPosition = cellCenter + new PointF(0, cellRect.Height / 2 + textMargin);
                        c.DrawText(textOptions, text, font, color, textPosition);

                        //avatars
                        var avatarsForCell = avatarsByCell[(x, y)].ToList();
                        if (avatarsForCell.Any())
                        {
                            //draw all the avatars on one image, then resize if necessary
                            var avatarWidth = avatarsForCell.First().Width;
                            var avatars = new Image<Rgba32>(avatarWidth * avatarsForCell.Count, avatarWidth);
                            avatars.Mutate(ac =>
                            {
                                for (var i = 0; i < avatarsForCell.Count; i++)
                                    ac.DrawImage(avatarsForCell[i], 1, new Point(i * avatarWidth, 0));
                                if (avatars.Width > cellRect.Width)
                                    ac.Resize((Size) (avatars.Size() * (cellRect.Width / avatars.Width)));
                            });
                            //use the average color from the avatars as cell background
                            var blurryAvatars = avatars.Clone();
                            blurryAvatars.Mutate(i => i.GaussianBlur(10));
                            var averageColor = blurryAvatars[blurryAvatars.Width / 2, blurryAvatars.Height / 2];
                            c.Fill(averageColor, cellRect);
                            var position = cellCenter - avatars.Size() / 2;
                            c.DrawImage(avatars, 1, (Point) position);
                        }
                        else
                        {
                            c.Fill(color, cellRect);
                        }
                    }
                });
                canvas.SaveAsPng(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream UpDownGif(string text, byte[]? avatarData, string baseFilename)
        {
            //credits to https://twitter.com/reedjeyy for base images, font and constants
            //constants
            const int textRightMargin = 1037;
            const int avatarLeftMargin = 1145;
            const int fontHeightCorrection = 22;
            const float fadeDuration = 11;
            const int fadeStartIndex = 65;
            var font = GothamRoundedLight.CreateFont(94);

            var output = new MemoryStream();
            using (var resource = Resource.Load(baseFilename))
            using (var baseImage = Image.Load<Rgba32>(resource))
            using (var textImage = new Image<Rgba32>(baseImage.Width, baseImage.Height))
            {
                //text overlay
                var heightExtent = textImage.Height / 2;
                textImage.Mutate(c =>
                {
                    var avatarImage = avatarData == null ? null : CropCircle(avatarData);
                    var t = new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        WrapTextWidth = textRightMargin
                    };
                    var textPosition = new PointF(0, heightExtent - fontHeightCorrection);
                    c.DrawText(t, text, font, Rgba32.White, textPosition);
                    if (avatarImage == null) return;
                    c.DrawImage(avatarImage, 1, new Point(avatarLeftMargin, heightExtent - avatarImage.Height / 2));
                });

                //apply overlay to each frame with proper opacity
                for (var i = 0; i < baseImage.Frames.Count; i++)
                {
                    var frame = baseImage.Frames.ExportFrame(i);
                    var opacity = 1 - Math.Clamp(1 / fadeDuration * (i - fadeStartIndex), 0, 1);
                    frame.Mutate(c => c.DrawImage(textImage, opacity));
                    baseImage.Frames.InsertFrame(i, frame.Frames.Single());
                }

                baseImage.SaveAsGif(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream Ok(string text, byte[] avatarData)
        {
            const float leftMargin = 4;
            const float topMargin = 4;
            const float avatarWidth = 70;
            const float avatarHeight = 74;
            const float textX = 80;
            const float textY = 12;
            var avatarSize = new SizeF(avatarWidth, avatarHeight);
            var avatarPosition = new PointF(leftMargin, topMargin);
            var textPosition = new PointF(textX, textY);
            var font = MsSansSerif.CreateFont(10);

            var output = new MemoryStream();
            using (var resource = Resource.Load("angry.png"))
            using (var baseImage = Image.Load<Rgba32>(resource))
            using (var avatar = Image.Load(avatarData))
            {
                var textOptions = new TextGraphicsOptions
                {
                    WrapTextWidth = baseImage.Width - textPosition.X
                };
                avatar.Mutate(a => a.Resize((Size) avatarSize).Dither(new BayerDither2x2()));
                baseImage.Mutate(i =>
                {
                    i.DrawImage(avatar, 1, (Point) avatarPosition);
                    i.DrawText(textOptions, text, font, Rgba32.Black, textPosition);
                    i.Resize(baseImage.Size() * 3);
                });
                baseImage.SaveAsPng(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream Text(string name, string text)
        {
            const int textX = 19;
            const int nameY = 252;
            const int textY = 269;
            const int textRightMargin = 20;
            var namePosition = new PointF(textX, nameY);
            var textPosition = new PointF(textX, textY);
            var nameFont = HelveticaNeueMedium.CreateFont(14);
            var textFont = HelveticaNeue.CreateFont(14.75F);
            var textColor = new Rgba32(4, 4, 4);

            var output = new MemoryStream();
            using (var resource = Resource.Load("text1.png"))
            using (var baseImage = Image.Load<Rgba32>(resource))
            {
                var textOptions = new TextGraphicsOptions
                {
                    WrapTextWidth = baseImage.Width - textPosition.X - textRightMargin
                };
                baseImage.Mutate(i =>
                {
                    i.DrawText(textOptions, name, nameFont, textColor, namePosition);
                    i.DrawText(textOptions, text, textFont, textColor, textPosition);
                    i.Resize(baseImage.Size() * 3);
                });
                baseImage.SaveAsPng(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        private Image<Rgba32> CropCircle(byte[] imageData)
        {
            const int avatarSize = 128;
            var image = imageData != null ? Image.Load(imageData) : new Image<Rgba32>(Configuration.Default, avatarSize, avatarSize, Rgba32.Red);
            image.Mutate(i =>
            {
                i.Resize(avatarSize, avatarSize);
                ApplyRoundedCorners(i, avatarSize / 2);
            });
            return image;
        }

        private static IImageProcessingContext<Rgba32> ApplyRoundedCorners(IImageProcessingContext<Rgba32> ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = size.Width - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = size.Height - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);
            IPathCollection corners = new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            return ctx.Fill(
                new GraphicsOptions(true) { BlenderMode = PixelBlenderMode.Src }, //use overlay colors
                Rgba32.Transparent, 
                corners);
        }

        public MemoryStream Quadrant(List<(Random rng, byte[])> rngAndAvatarDatas, string opt1, string opt2, string opt3,
            string opt4)
        {
            //constants
            const int s = 1280; //picture size
            const int margin = s / 25;
            const int textMargin = s / 100;
            var color = Rgba32.Black;

            //computed variables
            var parametersSeed = $"{opt1}{opt2}{opt3}{opt4}".ToLower();
            const int med = s / 2;
            var rendererOptions = new RendererOptions(Arial.CreateFont(35, FontStyle.Bold));
            var opt3Extent = TextMeasurer.Measure(opt3, rendererOptions).Width / 2;
            var opt4Extent = TextMeasurer.Measure(opt4, rendererOptions).Width / 2;
            var output = new MemoryStream();
            using (var canvas = new Image<Rgba32>(s, s))
            {
                canvas.Mutate(c =>
                {
                    //background
                    c.Fill(Rgba32.White);
                    var lineSegment = new LinearLineSegment(new PointF(med, margin), new PointF(med, s - margin));
                    var verticalLine = new Polygon(lineSegment);
                    var horizontalLine = verticalLine.Rotate((float)(Math.PI / 2));
                    c.Draw(new Pen<Rgba32>(color, 5), new PathCollection(verticalLine, horizontalLine));

                    //avatars
                    foreach (var (rng, avatarData) in rngAndAvatarDatas)
                    {
                        var saltedRng = (parametersSeed, rng.Next()).ToRandom();
                        using (var avatar = CropCircle(avatarData))
                        {
                            var avatarPosition = new Point(saltedRng.Next(s - avatar.Width),
                                saltedRng.Next(s - avatar.Height));
                            c.DrawImage(avatar, 1, avatarPosition);
                        }
                    }

                    //text
                    var t = new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    c.DrawText(t, opt1, rendererOptions.Font, color, new PointF(med, textMargin));
                    c.DrawText(t, opt2, rendererOptions.Font, color, new PointF(med, s - textMargin * 2));
                    c.DrawText(t, opt3, rendererOptions.Font, color,
                        new PointF(opt3Extent + textMargin, med + textMargin));
                    c.DrawText(t, opt4, rendererOptions.Font, color,
                        new PointF(s - (opt4Extent + textMargin), med + textMargin));
                });
                canvas.SaveAsPng(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream Poly(List<(Random rng, byte[])> rngAndAvatars, string[] options)
        {
            //constants
            const int s = 1280; //picture size
            const float extent = s * .5F;
            var color = Rgba32.Black;
            const double polyRadius = s * 0.45;
            var center = new PointF(extent, extent);
            const float textMargin = 10;

            //computed variables
            var smallArial = Arial.CreateFont(28);
            var rendererOptions = new RendererOptions(smallArial);
            var poly = (IPath) new RegularPolygon(center, options.Length, (int) polyRadius, (float) Math.PI);
            var polyBounds = poly.Bounds.Location + poly.Bounds.Size / 2;
            var polyCenter = 2 * center - polyBounds;
            poly = poly.Translate(polyCenter - center);
            var vertices = poly.Flatten().Single().Points;
            var output = new MemoryStream();
            using (var canvas = new Image<Rgba32>(s, s))
            {
                canvas.Mutate(c =>
                {
                    //background
                    c.Fill(Rgba32.White);
                    c.Draw(new Pen<Rgba32>(color, 5), poly);
                    var t = new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var optionsAndVertices = options.EquiZip(vertices, (o, v) => (o, v));
                    foreach (var (option, vertex) in optionsAndVertices)
                    {
                        var tPosition = vertex + (vertex - polyCenter) * 0.1F;
                        var tExtent = TextMeasurer.Measure(option, rendererOptions) / 2;
                        var clampedPosition = new PointF(
                            Math.Clamp(tPosition.X, tExtent.Width + textMargin, s - tExtent.Width - textMargin),
                            Math.Clamp(tPosition.Y, tExtent.Height + textMargin, s - tExtent.Height - textMargin));
                        c.DrawText(t, option, smallArial, color, clampedPosition);
                    }

                    //avatars
                    foreach (var (rng, avatarData) in rngAndAvatars)
                    {
                        var saltedRng = (options, rng.Next()).ToRandom();
                        using (var avatar = CropCircle(avatarData))
                        {
                            var vIndex = saltedRng.Next(vertices.Count);
                            var vA = polyCenter;
                            var vB = vertices[vIndex];
                            var vC = vertices[(vIndex + 1) % vertices.Count];
                            var aPosition = RandomPointInTriangle(saltedRng, vA, vB, vC) - avatar.Size() / 2;
                            c.DrawImage(avatar, 1, new Point((int) aPosition.X, (int) aPosition.Y));
                        }
                    }
                });
                canvas.SaveAsPng(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream Rank(IList<(Random rng, byte[] avatar)> rngAndAvatars, string[] tiers)
        {
            if (!tiers.Any()) throw new Exception("need at least one tier");
            const int w = 1280;
            const int rowH = 128;
            const int rowHExtent = rowH / 2;
            const float headerW = w * 0.2f;
            const float valueW = w - headerW;

            var color = Rgba32.Black;
            var output = new MemoryStream();
            var rows = tiers.Length;
            var h = rowH * rows;
            var smallArial = Arial.CreateFont(28);
            var textOptions = new TextGraphicsOptions(true)
            {
                WrapTextWidth = headerW, 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            using var canvas = new Image<Rgba32>(w, h);

            var avatarsByCell = rngAndAvatars
                .Select(tuple => (rng: (tiers, tuple.rng.Next()).ToRandom(), tuple.avatar))
                .ToLookup(tuple => tuple.rng.Next(rows), tuple => CropCircle(tuple.avatar));
            canvas.Mutate(c =>
            {
                c.Fill(Rgba32.White);
                var pen = new Pen<Rgba32>(color, 5);
                c.DrawLines(pen, new PointF(headerW, 0), new PointF(headerW, h));
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var tier = tiers[rowIndex];
                    var rowY = rowH * rowIndex;
                    var rowCenter = rowY + rowHExtent;
                    c.DrawText(textOptions, tier, smallArial, color, new PointF(0, rowCenter));
                    if (rowY > 0) c.DrawLines(pen, new PointF(0, rowY), new PointF(w, rowY));

                    var avatarsForRow = avatarsByCell[rowIndex].ToList();
                    if (avatarsForRow.Any())
                    {
                        //draw all the avatars on one image, then resize if necessary
                        var avatarWidth = avatarsForRow.First().Width;
                        var avatars = new Image<Rgba32>(avatarWidth * avatarsForRow.Count, avatarWidth);
                        avatars.Mutate(ac =>
                        {
                            for (var i = 0; i < avatarsForRow.Count; i++)
                                ac.DrawImage(avatarsForRow[i], 1, new Point(i * avatarWidth, 0));
                            if (avatars.Width > valueW)
                                ac.Resize((Size) (avatars.Size() * (valueW / avatars.Width)));
                        });
                        var position = new Point((int) headerW, rowY);
                        c.DrawImage(avatars, 1, position);
                    }
                }
            });
            canvas.SaveAsPng(output);
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        public MemoryStream Imitate(byte[] avatarData, string username, string? text, byte[]? imageData)
        {
            const int avatarS = 128;
            const int imagePadding = 10;
            const int w = 720 + imagePadding * 2;
            const int bubbleXMargin = 10;
            const int bubblePadding = 20;
            const int bubbleW = w - avatarS - bubbleXMargin * 2;
            const int marginUnderName = 15;

            var nameColor = Rgba32.DarkRed;
            var textColor = Rgba32.Black;
            var output = new MemoryStream();
            var font = HelveticaNeue.CreateFont(28);
            var nameFont = HelveticaNeueMedium.CreateFont(28);
            var bubbleContentW = bubbleW - bubblePadding * 2;
            var textOptions = new TextGraphicsOptions(true)
            {
                WrapTextWidth = bubbleContentW, 
                VerticalAlignment = VerticalAlignment.Top
            };

            var textHeight = text != null
                ? TextMeasurer.Measure(text, new RendererOptions(font) {WrappingWidth = bubbleContentW}).Height
                : 0;
            using var image = imageData != null ? Image.Load<Rgba32>(imageData) : null;
            if (image?.Width > bubbleContentW)
                image.Mutate(i => i.Resize((Size) (image.Size() * ((float)bubbleContentW / image.Width))));
            var imageHeight = image?.Height ?? 0;
            var marginUnderImage = image != null ? marginUnderName : 0;
            var nameHeight = nameFont.Size;
            var bubbleContentH = bubblePadding * 2 + nameHeight + marginUnderName + 
                                 imageHeight + marginUnderImage + textHeight;
            var bubbleH = Math.Max(bubbleContentH, avatarS);
            var h = (int)bubbleH + imagePadding * 2;
            using var canvas = new Image<Rgba32>(w, h);
            canvas.Mutate(c =>
            {
                using var speechBubble = new Image<Rgba32>(bubbleW, (int) bubbleH);
                speechBubble.Mutate(sc =>
                {
                    sc.Fill(Rgba32.White);
                    ApplyRoundedCorners(sc, 10);
                });
                var avatarPos = new Point(imagePadding, imagePadding);
                var avatar = CropCircle(avatarData);
                c.DrawImage(avatar, 1, avatarPos);
                var speechBubblePos = avatarPos + new Size(bubbleXMargin + avatar.Width, 0);
                c.DrawImage(speechBubble, 1, speechBubblePos);
                var namePos = speechBubblePos + new Size(bubblePadding, bubblePadding);
                c.DrawText(username, nameFont, nameColor, namePos);
                var imagePosition = namePos + new Size(0, (int)(nameHeight + marginUnderName));
                if (image != null) c.DrawImage(image, 1, imagePosition);
                var textPosition = imagePosition + new Size(0, imageHeight + marginUnderImage);
                if (text != null) c.DrawText(textOptions, text, font, textColor, textPosition);
            });
            canvas.SaveAsPng(output);
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        private static PointF RandomPointInTriangle(Random rng, PointF a, PointF b, PointF c)
        {
            var r1 = (float) rng.NextDouble();
            var r2 = (float) rng.NextDouble();
            var result = (1 - (float) Math.Sqrt(r1)) * a +
                         (float) Math.Sqrt(r1) * (1 - r2) * b +
                         r2 * (float) Math.Sqrt(r1) * c;
            return result;
        }
    }
}