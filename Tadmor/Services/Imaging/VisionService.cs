using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DlibDotNet;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = OpenCvSharp.Point;

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
                var faces = await GetFaces(origSrcImg, origDstImg);
                var origSourceFace = faces.FirstOrDefault() ?? throw new Exception("no faces detected");
                var origDstFace = faces.Skip(1).FirstOrDefault() ?? throw new Exception("only one face detected");
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


        private async Task<List<List<Point2f>>> GetFaces(Mat srcImg, Mat dstImg)
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
                var faces = DetectFaces(detector, predictor, srcImg)
                    .Concat(DetectFaces(detector, predictor, dstImg))
                    .ToList();
                return faces;
            }
        }

        private static (Mat transformedImage, List<Point2f> transformedPoints) SimilarityTransform(int outputWidth,
            int outputHeight, List<Point2f> face, Mat img)
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
            {
                using (var array = InputArray.Create(eyeCornersDest))
                {
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
            using (var subdivision = new Subdiv2D(rect))
            {
                subdivision.Insert(averageFace);
                var triangulation = subdivision.GetTriangleList();
                foreach (var cell in triangulation)
                {
                    var p1 = new Point2f(cell.Item0, cell.Item1);
                    var p2 = new Point2f(cell.Item2, cell.Item3);
                    var p3 = new Point2f(cell.Item4, cell.Item5);
                    if (rect.Contains(p1) && rect.Contains(p2) && rect.Contains(p3))
                    {
                        var indexA = averageFace.IndexOf(p1);
                        var indexB = averageFace.IndexOf(p2);
                        var indexC = averageFace.IndexOf(p3);
                        var indexes = new[] {indexA, indexB, indexC};
                        var srcTri = indexes.Select(i => srcFace[i]).ToList();
                        var dstTri = indexes.Select(i => dstFace[i]).ToList();
                        var avgTri = indexes.Select(i => averageFace[i]).ToList();
                        var srcRect = Cv2.BoundingRect(srcTri);
                        var dstRect = Cv2.BoundingRect(dstTri);
                        var avgRect = Cv2.BoundingRect(avgTri);

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
            }
        }
    }
}