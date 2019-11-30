using System;
using System.Drawing;
using ImageMagick;

namespace Tadmor.Services.Imaging
{
    public static class ImageMagickExtensions
    {
        public static Drawables Composite(this Drawables drawables,
            IMagickImage image,
            Rectangle rectangle,
            Gravity gravity,
            CompositeOperator composite = CompositeOperator.Over)
        {
            var imgSize = image.GetSize();
            var boundsSize = rectangle.Size;
            var posDelta = GetGravityDelta(imgSize, boundsSize, gravity);
            var pos = rectangle.Location + posDelta;
            return drawables.Composite(pos.X, pos.Y, composite, image);
        }
        public static Drawables Composite(
            this Drawables drawables,
            IMagickImage image,
            Point position, 
            Gravity gravity = Gravity.Northwest,
            CompositeOperator composite=CompositeOperator.Over)
        {
            return drawables.Composite(image, new Rectangle(position, default), gravity, composite);
        }

        private static Size GetGravityDelta(Size imgSize, Size boundsSize, Gravity gravity)
        {
            var rectW = boundsSize.Width;
            var rectH = boundsSize.Height;
            var posDelta = gravity switch
            {
                Gravity.Undefined => new Size(),
                Gravity.Northwest => new Size(),
                Gravity.North => new Size((rectW - imgSize.Width) / 2, 0),
                Gravity.Northeast => new Size(rectW - imgSize.Width, 0),
                Gravity.West => new Size(0, (rectH - imgSize.Height) / 2),
                Gravity.Center => (boundsSize - imgSize) / 2,
                Gravity.East => new Size(rectW - imgSize.Width, (rectH - imgSize.Height) / 2),
                Gravity.Southwest => new Size(0, rectH - imgSize.Height),
                Gravity.South => new Size((rectW - imgSize.Width) / 2, rectH - imgSize.Height),
                Gravity.Southeast => boundsSize - imgSize,
                _ => throw new ArgumentOutOfRangeException(nameof(gravity))
            };
            return posDelta;
        }

        public static Drawables TextFill(this Drawables drawables,
            string text,
            Rectangle textRect,
            string font,
            TextAlignment textAlignment,
            MagickColor? textColor = default,
            MagickColor? backgroundColor = default)
        {
            var textGravity = textAlignment switch
            {
                TextAlignment.Center => Gravity.Center,
                TextAlignment.Left => Gravity.West,
                TextAlignment.Right => Gravity.East,
                _ => default
            };
            var textCanvas = CreateTextCanvas(text, "label", textRect.Width, textRect.Height, font, textGravity, default, textColor, backgroundColor);
            drawables.Composite(textRect.X, textRect.Y, CompositeOperator.Over, textCanvas);
            return drawables;
        }

        public static Drawables Caption(this Drawables drawables,
            string text,
            Rectangle textRect,
            string font,
            Gravity textGravity,
            double fontPointSize = 0,
            MagickColor? textColor = default,
            MagickColor? backgroundColor = default)
        {
            var textCanvas = GetCaption(text, textRect.Size.Width, textRect.Size.Height, font, textGravity, fontPointSize, textColor, backgroundColor);
            drawables.Composite(textRect.X, textRect.Y, CompositeOperator.Over, textCanvas);
            return drawables;
        }

        public static MagickImage GetCaption(string text, int width, int? height, string font, Gravity textGravity,
            double fontPointSize, MagickColor? textColor, MagickColor? backgroundColor)
        {
            var textCanvas = CreateTextCanvas(text, "caption", width, height, font, textGravity,
                fontPointSize, textColor, backgroundColor);
            return textCanvas;
        }

        private static MagickImage CreateTextCanvas(string text,
            string textType,
            int? width,
            int? height,
            string font,
            Gravity textGravity,
            double fontPointSize,
            MagickColor? textColor,
            MagickColor? backgroundColor)
        {
            return new MagickImage($"{textType}:{text}", new MagickReadSettings
            {
                FontFamily = font,
                Width = width,
                Height = height,
                Font = font,
                TextGravity = textGravity,
                FontPointsize = fontPointSize,
                FillColor = textColor ?? MagickColors.Black,
                BackgroundColor = backgroundColor ?? MagickColors.Transparent
            });
        }

        public static Drawables Label(
            this Drawables drawables,
            string text,
            Point textPos,
            string font,
            Gravity textGravity,
            double fontPointSize,
            Rectangle bounds = default,
            MagickColor? textColor = default,
            MagickColor? backgroundColor = default)
        {
            var textCanvas = CreateTextCanvas(text, "label", null, null, font, textGravity, fontPointSize, textColor, backgroundColor);
            var textSize = textCanvas.GetSize();
            textPos += GetGravityDelta(textSize, default, textGravity);
            if (bounds != default)
            {
                textPos = new Point(
                    Math.Clamp(textPos.X, bounds.X, bounds.X + bounds.Width - textSize.Width),
                    Math.Clamp(textPos.Y, bounds.Y, bounds.Y + bounds.Height - textSize.Height));
            }
            drawables.Composite(textPos.X, textPos.Y, CompositeOperator.Over, textCanvas);
            return drawables;
        }

        public static void SetOpacity(this IMagickImage image, float opacity)
        {
            image.Alpha(AlphaOption.Set);
            image.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, opacity);
        }

        public static Rectangle GetRectangle(this IMagickImage image)
        {
            return new Rectangle(default, image.GetSize());
        }

        public static Size GetSize(this IMagickImage image)
        {
            return new Size(image.Width, image.Height);
        }
    }
}