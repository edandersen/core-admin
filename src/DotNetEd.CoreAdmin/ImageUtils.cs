using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetEd.CoreAdmin
{
    public class ImageUtils
    {
        public static string WebBase64EncodeImageByteArrayOrNull(byte[] image)
        {
            if (image == null) return null;

            var byteSignaturesAndContentTypes = new Dictionary<byte[], string>();
            byteSignaturesAndContentTypes.Add(Encoding.ASCII.GetBytes("BM"), "image/bmp");
            byteSignaturesAndContentTypes.Add(Encoding.ASCII.GetBytes("GIF"), "image/gif");
            byteSignaturesAndContentTypes.Add(new byte[] { 137, 80, 78, 71 }, "image/png");
            byteSignaturesAndContentTypes.Add(new byte[] { 73, 73, 42 }, "image/tiff");
            byteSignaturesAndContentTypes.Add(new byte[] { 77, 77, 42 }, "image/tiff");
            byteSignaturesAndContentTypes.Add(new byte[] { 255, 216, 255, 224 }, "image/jpeg");
            byteSignaturesAndContentTypes.Add(new byte[] { 255, 216, 255, 225 }, "image/jpeg");

            var matchingSig = byteSignaturesAndContentTypes
                .Select(e => (KeyValuePair<byte[], string>?)e)
                .FirstOrDefault(bs => bs.Value.Key.SequenceEqual(image.Take(bs.Value.Key.Length)));

            if (matchingSig == null) return null;

            var imageString = $"data:{matchingSig.Value.Value};base64," + Convert.ToBase64String(image);

            return imageString;
        }
    }
}
