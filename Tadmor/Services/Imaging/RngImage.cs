using System;
using Tadmor.Extensions;

namespace Tadmor.Services.Imaging
{
    public class RngImage
    {
        public byte[] ImageData { get; }
        public Random Random { get; private set; }

        public RngImage(byte[] image, Random random)
        {
            ImageData = image;
            Random = random;
        }

        public RngImage Extend(object subSeed)
        {
            Random = (Random.Next(), subSeed).ToRandom();
            return this;
        }
    }
}