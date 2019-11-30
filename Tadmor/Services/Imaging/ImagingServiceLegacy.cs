using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Convolution;
using SixLabors.ImageSharp.Processing.Dithering;
using SixLabors.ImageSharp.Processing.Dithering.Ordered;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;
using Tadmor.Resources;

namespace Tadmor.Services.Imaging
{
    [SingletonService]
    public class ImagingServiceLegacy
    {
        private static readonly FontFamily Arial = SystemFonts.Find("Arial");

        private static readonly FontFamily HelveticaNeue =
            new FontCollection().Install(Resource.Load("HelveticaNeue.ttf"));

        private static readonly FontFamily HelveticaNeueMedium =
            new FontCollection().Install(Resource.Load("HelveticaNeueMedium.ttf"));

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

        public MemoryStream Stack(IEnumerable<byte[]> imageDatas, int margin, int padding)
        {
            var images = imageDatas
                .Select(Image.Load<Rgba32>)
                .ToList();
            var w = images.Max(i => i.Width) + margin*2;
            var h = images.Sum(i => i.Height) + margin * 2 + padding * (images.Count - 1);
            var output = new MemoryStream();
            using var canvas = new Image<Rgba32>(w, h);
            canvas.Mutate(c =>
            {
                var y = margin;
                foreach (var image in images)
                {
                    c.DrawImage(image, 1, new Point(margin, y));
                    y += image.Height + padding;
                }
            });
            canvas.SaveAsPng(output);
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }
    }
}