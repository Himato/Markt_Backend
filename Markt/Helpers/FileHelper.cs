using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markt.Helpers
{

    public class FileHelper    
    {
        public static bool IsImage(byte[] bytes)
        {
            var headers = new List<byte[]>
            {
                Encoding.ASCII.GetBytes("BM"),      // BMP
                Encoding.ASCII.GetBytes("GIF"),     // GIF
                new byte[] { 137, 80, 78, 71 },     // PNG
                new byte[] { 73, 73, 42 },          // TIFF
                new byte[] { 77, 77, 42 },          // TIFF
                new byte[] { 255, 216, 255 },  // All JPG
                //new byte[] { 255, 216, 255, 224 },  // JPEG
                //new byte[] { 255, 216, 255, 225 }   // JPEG CANON
            };

            return headers.Any(x => x.SequenceEqual(bytes.Take(x.Length)));
        }
    }
}
