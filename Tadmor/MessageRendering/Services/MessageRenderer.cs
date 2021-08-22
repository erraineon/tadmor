using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Tadmor.MessageRendering.Interfaces;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Services
{
    public static class Resources
    {
        public static Stream Load(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            var resourcePath = assembly.GetManifestResourceNames()
                                   .SingleOrDefault(str => str.EndsWith(resourceName)) ??
                               throw new Exception(
                                   $"resource {resourceName} not found in assembly {assembly.FullName}");
            var resourceStream = assembly.GetManifestResourceStream(resourcePath)!;
            return resourceStream;
        }
    }

    public class MessageRenderer : IMessageRenderer
    {
        private readonly Font _contentFont;
        private readonly Font _authorFont;
        private readonly DrawingOptions _noAntiAliasDrawingOptions;

        public MessageRenderer()
        {
            var fontCollection = new FontCollection();
            var contentFamily = fontCollection.Install(Resources.Load("Mx437_DOS-V_re_JPN24.ttf"));
            _contentFont = contentFamily.CreateFont(24);
            var authorFamily = fontCollection.Install(Resources.Load("Mx437_IBM_XGA-AI_12x20.ttf"));
            _authorFont = authorFamily.CreateFont(20);
            _noAntiAliasDrawingOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = false }
            };
        }

        public byte[] RenderConversation(IList<DrawableMessage> messages)
        {
            var messagesMargin = 20;
            var messageImages = messages.Select(DrawMessage).ToList();
            if (!messageImages.Any()) throw new Exception("no images were drawn");
            var canvasPadding = new Size(20, 20);
            var canvasWidth = messageImages.Max(i => i.Width) +
                              canvasPadding.Width * 2;
            var canvasHeight = messageImages.Sum(i => i.Height) +
                                      messagesMargin * (messageImages.Count - 1) +
                                      canvasPadding.Height * 2;
            var canvas = new Image<Rgba32>(canvasWidth, canvasHeight, Color.Blue);
            canvas.Mutate(i =>
            {
                var currentPosition = Point.Empty + canvasPadding;
                foreach (var messageImage in messageImages)
                {
                    i.DrawImage(messageImage, currentPosition, 1);
                    var imageBorderBounds = new Rectangle(currentPosition, messageImage.Size());
                    i.Draw(_noAntiAliasDrawingOptions, Color.White, 1, imageBorderBounds);
                    currentPosition += new Size(0, messageImage.Height + messagesMargin);
                }
            });
            var memoryStream = new MemoryStream();
            canvas.SaveAsPng(memoryStream);
            return memoryStream.ToArray();
        }

        private Image DrawMessage(DrawableMessage message)
        {
            var textBoxWidth = 410;
            var textBoxImageContentMargin = 20;

            // get content size
            var contentHeight = 0;
            if (message.Content is { } content && !string.IsNullOrWhiteSpace(content))
            {
                var contentRendererOptions = new RendererOptions(_contentFont)
                {
                    WrappingWidth = textBoxWidth
                };
                var messageContent = message.Content;
                contentHeight =
                    (int)Math.Ceiling(TextMeasurer.MeasureBounds(messageContent, contentRendererOptions).Height);
            }

            var contentSize = new Size(textBoxWidth, contentHeight);

            // get image size
            var image = default(Image);
            if (message.Image is { } imageBytes)
            {
                image = Image.Load(imageBytes);
                image.Mutate(i =>
                {
                    i.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Min,
                        Size = new Size(textBoxWidth, 0)
                    });
                });
            }

            var imageSize = image?.Size() ?? new Size(textBoxWidth, 0);
            var textBoxImageContentMarginSize = imageSize.Height > 0 && contentSize.Height > 0
                ? new Size(textBoxWidth, textBoxImageContentMargin)
                : new Size(textBoxWidth, 0);

            // get text box size: now all the variable sizes are known
            var textBoxSize = new Size(textBoxWidth,
                imageSize.Height + textBoxImageContentMarginSize.Height + contentSize.Height);

            var canvasPadding = new Point(20, 20);

            var avatar = message.Avatar != null ? Image.Load(message.Avatar) : new Image<Rgba32>(128, 128);
            Avatarize(avatar, 128);
            var avatarBounds = avatar.Bounds();
            avatarBounds.Offset(canvasPadding);

            var avatarAuthorMargin = 10;
            var authorBounds = new Rectangle(
                new Point(avatarBounds.Left, avatarBounds.Bottom + avatarAuthorMargin),
                new Size(avatarBounds.Width, 20));

            var avatarTextBoxMargin = 20;
            var textBoxBounds = new Rectangle(
                new Point(avatarBounds.Right + avatarTextBoxMargin, avatarBounds.Top),
                textBoxSize);

            var canvasSize = new Size(
                textBoxBounds.Right + canvasPadding.X,
                Math.Max(authorBounds.Bottom, textBoxBounds.Bottom) + canvasPadding.Y
            );

            var noAntiAliasGraphicsOptions = new GraphicsOptions { Antialias = false };
            var sharpDrawingOptions = new DrawingOptions
            {
                GraphicsOptions = noAntiAliasGraphicsOptions,
                TextOptions = new TextOptions { WrapTextWidth = textBoxWidth }
            };

            var authorDrawingOptions = new DrawingOptions
            {
                GraphicsOptions = noAntiAliasGraphicsOptions,
                TextOptions = new TextOptions
                {
                    WrapTextWidth = authorBounds.Width,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            var canvas = new Image<Rgba32>(canvasSize.Width, canvasSize.Height, Color.Black);
            canvas.Mutate(c =>
            {
                // avatar
                c.Draw(sharpDrawingOptions, Color.White, 1, avatarBounds);
                c.DrawImage(avatar, avatarBounds.Location, 1);
                avatarBounds.Inflate(new Size(2));
                c.Draw(sharpDrawingOptions, Color.White, 1, avatarBounds);

                // author
                c.DrawText(authorDrawingOptions, message.AuthorName, _authorFont, Color.Cyan, authorBounds.Location);

                // content
                var currentContentLocation = textBoxBounds.Location;
                if (imageSize.Height > 0)
                {
                    c.DrawImage(image, currentContentLocation, 1);
                    currentContentLocation += new Size(0, imageSize.Height + textBoxImageContentMargin);
                }

                if (contentSize.Height > 0)
                    c.DrawText(sharpDrawingOptions, message.Content, _contentFont, Color.White, currentContentLocation);
            });
            return canvas;
        }

        public void Avatarize(Image baseImage, int avatarSize)
        {
            var edgesImage = baseImage.Clone(im =>
            {
                im.DetectEdges(EdgeDetector2DKernel.SobelKernel, false);
                im.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Stretch,
                    Size = new Size(avatarSize, avatarSize)
                });
                im.Invert();
                im.BinaryThreshold(0.5f);
            });
            baseImage.Mutate(im =>
            {
                im.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Stretch,
                    Size = new Size(avatarSize, avatarSize)
                });
                ApplyPc88PaletteQuantizer(im, KnownDitherings.Bayer2x2);
            });
        }

        private static void ApplyPc88PaletteQuantizer(IImageProcessingContext im, IDither dithering)
        {
            var pc88Palette = new ReadOnlyMemory<Color>(new[]
            {
                Color.White,
                Color.Black,
                Color.Red,
                Color.Blue,
                Color.Yellow,
                Color.Lime,
                Color.Magenta,
                Color.Cyan
            });
            var quantizer = new PaletteQuantizer(pc88Palette, new QuantizerOptions
            {
                Dither = dithering,
                DitherScale = 0.3f
            });
            im.Quantize(quantizer);
        }
    }
}