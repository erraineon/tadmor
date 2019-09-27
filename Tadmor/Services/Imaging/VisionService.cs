using DlibDotNet;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Point = OpenCvSharp.Point;
using Size = SixLabors.Primitives.Size;

namespace Tadmor.Services.Imaging
{
    public class VisionService
    {
        private Array2D<RgbPixel> ToArray(Mat mat)
        {
            var array = new Array2D<RgbPixel>(mat.Rows, mat.Cols);
            using (var mat3 = new MatOfByte3(mat))
            {
                var indexer = mat3.GetIndexer();
                for (var i = 0; i < array.Rows; i++)
                {
                    var destRow = array[i];
                    for (var j = 0; j < array.Columns; j++)
                    {
                        var color = indexer[i, j];
                        destRow[j] = new RgbPixel(color.Item2, color.Item1, color.Item0);
                    }
                }
            }

            return array;
        }

        private List<List<Point2f>> DetectFaces(FrontalFaceDetector detector, ShapePredictor predictor, Mat mat)
        {
            using (var image = ToArray(mat))
            {
                var points = detector.Operator(image)
                    .Select(rectangle => predictor.Detect(image, rectangle))
                    .Where(shape => shape.Parts > 2)
                    .Select(shape => Enumerable.Range(0, (int) shape.Parts)
                        .Select(i => shape.GetPart((uint) i))
                        .Select((p, i) => new Point2f(p.X, p.Y))
                        .ToList())
                    .ToList();
                return points;
            }
        }

        public async Task<MemoryStream> Morph(byte[] source, byte[] dest)
        {
            const int outputWidth = 300;
            const int outputHeight = 300;

            using (var morphImg = Mat.Zeros(outputWidth, outputHeight, MatType.CV_32FC3).ToMat())
            using (var origSrcImg = Mat.FromImageData(source))
            using (var origDstImg = Mat.FromImageData(dest))
            {
                var facesLookup = await DetectFaces(new[]{ origSrcImg, origDstImg });
                var faces = facesLookup.Select(g => g.FirstOrDefault()).ToList();
                if (faces.Count < 2) throw new Exception("not enough faces");
                var (origSourceFace, origDstFace) = (faces[0], faces[1]);
                origSrcImg.ConvertTo(origSrcImg, MatType.CV_32F);
                origDstImg.ConvertTo(origDstImg, MatType.CV_32F);
                var (srcImg, sourceFace) = SimilarityTransform(outputWidth, outputHeight, origSourceFace, origSrcImg);
                var (dstImg, dstFace) = SimilarityTransform(outputWidth, outputHeight, origDstFace, origDstImg);

                var boundingPoints = new[]
                {
                    new Point2f(0, 0),
                    new Point2f(outputWidth / 2F, 0),
                    new Point2f(outputWidth - 2, 0),
                    new Point2f(0, outputHeight / 2F),
                    new Point2f(outputWidth - 2, outputHeight / 2F),
                    new Point2f(0, outputHeight - 2),
                    new Point2f(outputWidth / 2F, outputHeight - 2),
                    new Point2f(outputWidth - 2, outputHeight - 2)
                };
                sourceFace.AddRange(boundingPoints);
                dstFace.AddRange(boundingPoints);

                var output = new MemoryStream();
                using (srcImg)
                using (dstImg)
                using (var outputImage = new Image<Rgba32>(outputWidth, outputHeight))
                {
                    for (var alpha = 0F; alpha < 1; alpha += 0.05F)
                    {
                        Morph(sourceFace, dstFace, alpha, morphImg, srcImg, dstImg);
                        var frame = Image.Load<Rgba32>(morphImg.ToBytes());
                        outputImage.Frames.AddFrame(frame.Frames.First());
                    }

                    outputImage.Frames.RemoveFrame(0);
                    outputImage.Frames.First().MetaData.FrameDelay = 50;
                    outputImage.Frames.Last().MetaData.FrameDelay = 50;
                    outputImage.SaveAsGif(output);
                }

                output.Seek(0, SeekOrigin.Begin);
                return output;
            }
        }


        private async Task<ILookup<Mat, List<Point2f>>> DetectFaces(IEnumerable<Mat> images)
        {
            const string faceModelPath = "facemodel.dat";
            if (!File.Exists(faceModelPath))
            {
                var modelUrl = "https://github.com/AKSHAYUBHAT/TensorFace/raw/master/" +
                               "openface/models/dlib/shape_predictor_68_face_landmarks.dat";
                using (var modelStream = await new HttpClient().GetStreamAsync(modelUrl))
                using (var fileStream = File.OpenWrite(faceModelPath))
                {
                    await modelStream.CopyToAsync(fileStream);
                }
            }

            using (var detector = Dlib.GetFrontalFaceDetector())
            using (var predictor = ShapePredictor.Deserialize(faceModelPath))
            {
                var faces = images
                    .SelectMany(image => DetectFaces(detector, predictor, image), (image, face) => (image, face))
                    .ToLookup(t => t.image, t => t.face);
                return faces;
            }
        }

        private static (Mat transformedImage, List<Point2f> transformedPoints) SimilarityTransform(int outputWidth,
            int outputHeight, IList<Point2f> face, Mat img)
        {
            var s60 = (float) Math.Sin(60 * Math.PI / 180.0);
            var c60 = (float) Math.Cos(60 * Math.PI / 180.0);

            Point2f[] ToEquilateral(Point2f a, Point2f b)
            {
                var dx = a.X - b.X;
                var dy = a.Y - b.Y;
                return new[] {a, b, new Point2f(c60 * dx - s60 * dy + b.X, s60 * dx + c60 * dy + b.Y)};
            }

            var eyeCornersSource = ToEquilateral(face[36], face[45]);
            var eyeCornersDest = ToEquilateral(
                new Point2f(0.3F * outputWidth, outputHeight / 3f),
                new Point2f(0.7F * outputWidth, outputHeight / 3f));

            using (var inputArray = InputArray.Create(eyeCornersSource))
            using (var array = InputArray.Create(eyeCornersDest))
            using (var transform = Cv2.EstimateRigidTransform(inputArray, array, false))
            {
                var transformedImage = Mat.Zeros(outputWidth, outputHeight, MatType.CV_32FC3).ToMat();
                Cv2.WarpAffine(img, transformedImage, transform, transformedImage.Size());
                using (var src = InputArray.Create(face))
                using (var dst = new Mat())
                {
                    Cv2.Transform(src, dst, transform);
                    var transformedPoints = new Point2f[dst.Rows * dst.Cols];
                    dst.GetArray(0, 0, transformedPoints);
                    return (transformedImage, transformedPoints.ToList());
                }
            }
        }

        private static void Morph(List<Point2f> srcFace, List<Point2f> dstFace, float alpha, Mat morphImage, Mat srcImg,
            Mat dstImg)
        {
            var averageFace = srcFace
                .Zip(dstFace, (p1, p2) => new Point2f(
                    (1 - alpha) * p1.X + alpha * p2.X,
                    (1 - alpha) * p1.Y + alpha * p2.Y
                ))
                .ToList();
            var rect = new Rect(0, 0, morphImage.Cols, morphImage.Rows);
            var indexesList = GetDelaunayTriangulationIndexes(rect, averageFace);

            foreach (var indexes in indexesList)
            {
                var srcTri = indexes.Select(i => srcFace[i]).ToList();
                var dstTri = indexes.Select(i => dstFace[i]).ToList();
                var avgTri = indexes.Select(i => averageFace[i]).ToList();
                var srcRect = Cv2.BoundingRect(srcTri).Intersect(new Rect(0, 0, srcImg.Width, srcImg.Height));
                var dstRect = Cv2.BoundingRect(dstTri).Intersect(new Rect(0, 0, dstImg.Width, dstImg.Height));
                var avgRect = Cv2.BoundingRect(avgTri).Intersect(new Rect(0, 0, morphImage.Width, morphImage.Height));

                var srcOffsetRect = new List<Point2f>();
                var dstOffsetRect = new List<Point2f>();
                var avgOffsetRect = new List<Point2f>();
                var avgOffsetRectInt = new List<Point>();
                for (var i = 0; i < 3; i++)
                {
                    srcOffsetRect.Add(new Point2f(srcTri[i].X - srcRect.X, srcTri[i].Y - srcRect.Y));
                    dstOffsetRect.Add(new Point2f(dstTri[i].X - dstRect.X, dstTri[i].Y - dstRect.Y));
                    avgOffsetRect.Add(new Point2f(avgTri[i].X - avgRect.X, avgTri[i].Y - avgRect.Y));
                    avgOffsetRectInt.Add(new Point(avgTri[i].X - avgRect.X, avgTri[i].Y - avgRect.Y));
                }

                using (var mask = Mat.Zeros(avgRect.Height, avgRect.Width, MatType.CV_32FC3).ToMat())
                {
                    var scalar = new Scalar(1, 1, 1);
                    Cv2.FillConvexPoly(mask, avgOffsetRectInt, scalar, LineTypes.AntiAlias);
                    var srcImgRect = new Mat();
                    var dstImgRect = new Mat();
                    srcImg[srcRect].CopyTo(srcImgRect);
                    dstImg[dstRect].CopyTo(dstImgRect);

                    void ApplyAffineTransform(Mat warpImage, Mat imgRect, List<Point2f> offsetRect)
                    {
                        using (var warpMat = Cv2.GetAffineTransform(offsetRect, avgOffsetRect))
                        {
                            Cv2.WarpAffine(imgRect, warpImage, warpMat, warpImage.Size(),
                                InterpolationFlags.Linear,
                                BorderTypes.Reflect101);
                        }
                    }

                    using (var warpImage1 = Mat.Zeros(avgRect.Height, avgRect.Width, srcImgRect.Type()).ToMat())
                    using (var warpImage2 = Mat.Zeros(avgRect.Height, avgRect.Width, dstImgRect.Type()).ToMat())
                    {
                        ApplyAffineTransform(warpImage1, srcImgRect, srcOffsetRect);
                        ApplyAffineTransform(warpImage2, dstImgRect, dstOffsetRect);

                        using (var avgImgRect = ((1.0 - alpha) * warpImage1 + alpha * warpImage2).ToMat())
                        {
                            Cv2.Multiply(avgImgRect, mask, avgImgRect);
                            Cv2.Multiply(morphImage[avgRect], scalar - mask, morphImage[avgRect]);
                            morphImage[avgRect] = morphImage[avgRect] + avgImgRect;
                        }
                    }
                }
            }
        }

        private static IEnumerable<int[]> GetDelaunayTriangulationIndexes(Rect rect, List<Point2f> points)
        {
            rect = Cv2.BoundingRect(points).Union(rect);
            using (var subdivision = new Subdiv2D(rect))
            {
                subdivision.Insert(points);
                var triangulation = subdivision.GetTriangleList();
                foreach (var cell in triangulation)
                {
                    var p1 = new Point(cell.Item0, cell.Item1);
                    var p2 = new Point(cell.Item2, cell.Item3);
                    var p3 = new Point(cell.Item4, cell.Item5);
                    if (rect.Contains(p1) && rect.Contains(p2) && rect.Contains(p3))
                    {
                        var indexA = points.IndexOf(p1);
                        var indexB = points.IndexOf(p2);
                        var indexC = points.IndexOf(p3);
                        var indexes = new[] {indexA, indexB, indexC};
                        yield return indexes;
                    }
                }
            }
        }

        public async Task<MemoryStream> Swap(IList<byte[]> images)
        {
            var imgs = images.Select(image => Mat.FromImageData(image)).ToList();
            var facesLookup = (await DetectFaces(imgs));
                if (facesLookup.Count < 2) throw new Exception("not enough faces");

            var img1 = facesLookup.First();
            var img2 = facesLookup.Last();
            var output1 = Swap(img1.Key, img1.First(), img2.Key, img2.Last());
            var output2 = Swap(img2.Key, img2.Last(), img1.Key, img1.First());

            output1.Mutate(i =>
            {
                var currentSize = output1.Size();
                var newSizeVert = new Size(Math.Max(output1.Width, output2.Width), output1.Height + output2.Height);
                var newSizeHor = new Size(output1.Width + output2.Width, Math.Max(output1.Height, output2.Height));
                var (newSize, location) =
                    newSizeVert.Height + newSizeVert.Width > newSizeHor.Height + newSizeHor.Width
                        ? (newSizeHor, new SixLabors.Primitives.Point(currentSize.Width, 0))
                        : (newSizeVert, new SixLabors.Primitives.Point(0, currentSize.Height));
                i.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.BoxPad,
                    Size = newSize,
                    Position = AnchorPositionMode.TopLeft
                });
                i.DrawImage(output2, 1, location);
            });

            var output = new MemoryStream();
            output1.SaveAsJpeg(output);
            output.Seek(0, SeekOrigin.Begin);
            return output;

        }

        private Image<Rgba32> Swap(Mat img1, IList<Point2f> points1, Mat img2, IList<Point2f> points2)
        {
            var img1Warped = img2.Clone();
            img1.ConvertTo(img1, MatType.CV_32F);
            img1Warped.ConvertTo(img1Warped, MatType.CV_32F);

            var rect = new Rect(0, 0, img1Warped.Cols, img1Warped.Rows);
            var hullIndex = Cv2.ConvexHullIndices(points2);

            var hull1 = hullIndex.Select(i => points1[i]).ToList();
            var hull2 = hullIndex.Select(i => points2[i]).ToList();

            var dt = GetDelaunayTriangulationIndexes(rect, hull2).ToList();

            foreach (var triangleIndexes in dt)
            {
                var t1 = triangleIndexes.Select(i => hull1[i]).ToList();
                var t2 = triangleIndexes.Select(i => hull2[i]).ToList();
                WarpTriangle(img1, img1Warped, t1, t2);
            }

            var hull8U = hull2.Select(p => new Point((int) p.X, (int) p.Y)).ToList();

            using (var mask = Mat.Zeros(img2.Rows, img2.Cols, MatType.CV_8UC3).ToMat())
            {
                Cv2.FillConvexPoly(mask, hull8U, new Scalar(255, 255, 255));
                var r = Cv2.BoundingRect(hull2).Intersect(rect);
                var center = r.Location + new Point(r.Width / 2, r.Height / 2);
                img1Warped.ConvertTo(img1Warped, MatType.CV_8UC3);
                img2.ConvertTo(img2, MatType.CV_8UC3);
                using (var outputMat = new Mat())
                {
                    Cv2.SeamlessClone(img1Warped, img2, mask, center, outputMat, SeamlessCloneMethods.NormalClone);
                    return Image.Load(outputMat.ToBytes());
                }
            }
        }

        private void WarpTriangle(Mat img1, Mat img2, List<Point2f> t1, List<Point2f> t2)
        {
            var r1 = Cv2.BoundingRect(t1).Intersect(new Rect(0, 0, img1.Width, img1.Height));
            var r2 = Cv2.BoundingRect(t2).Intersect(new Rect(0, 0, img2.Width, img2.Height));
            if (r1 == Rect.Empty || r2 == Rect.Empty) return;

            var t1Rect = new List<Point2f>();
            var t2Rect = new List<Point2f>();
            var t2RectInt = new List<Point>();
            for (var i = 0; i < 3; i++)
            {
                t1Rect.Add(new Point2f(t1[i].X - r1.X, t1[i].Y - r1.Y));
                t2Rect.Add(new Point2f(t2[i].X - r2.X, t2[i].Y - r2.Y));
                t2RectInt.Add(new Point(t2[i].X - r2.X, t2[i].Y - r2.Y));
            }

            using (var mask = Mat.Zeros(r2.Height, r2.Width, MatType.CV_32FC3).ToMat())
            {
                var scalar = new Scalar(1, 1, 1);
                Cv2.FillConvexPoly(mask, t2RectInt, scalar, LineTypes.AntiAlias);
                using (var img1Rect = new Mat())
                {
                    img1[r1].CopyTo(img1Rect);
                    var img2Rect = Mat.Zeros(r2.Height, r2.Width, img1Rect.Type()).ToMat();
                    using (var warpMat = Cv2.GetAffineTransform(t1Rect, t2Rect))
                    {
                        Cv2.WarpAffine(img1Rect, img2Rect, warpMat, img2Rect.Size(),
                            InterpolationFlags.Linear,
                            BorderTypes.Reflect101);
                        Cv2.Multiply(img2Rect, mask, img2Rect);
                        Cv2.Multiply(img2[r2], scalar - mask, img2[r2]);
                        img2[r2] = img2[r2] + img2Rect;
                    }
                }
            }
        }
    }
}