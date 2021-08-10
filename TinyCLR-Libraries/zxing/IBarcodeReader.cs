using System;

#if !SILVERLIGHT
#if !UNITY
using System.Collections;
using System.Drawing;
//using Microsoft.SPOT;

#endif
#else
using System.Windows.Media.Imaging;
#endif

namespace ZXing
{
   public delegate void ResultPointHandler(ResultPoint resultPoint);
   public delegate void ResultHandler(Result result);

   /// <summary>
   /// Interface for a smart class to decode the barcode inside a bitmap object
   /// </summary>
   public interface IBarcodeReader
   {
      /// <summary>
      /// event is executed when a result point was found
      /// </summary>
      event ResultPointHandler ResultPointFound;

      /// <summary>
      /// event is executed when a result was found via decode
      /// </summary>
      event ResultHandler ResultFound;

      /// <summary>
      /// Gets or sets a flag which cause a deeper look into the bitmap
      /// </summary>
      /// <value>
      ///   <c>true</c> if [try harder]; otherwise, <c>false</c>.
      /// </value>
      bool TryHarder { get; set; }

      /// <summary>
      /// Image is a pure monochrome image of a barcode. Doesn't matter what it maps to;
      /// use {@link Boolean#TRUE}.
      /// </summary>
      /// <value>
      ///   <c>true</c> if monochrome image of a barcode; otherwise, <c>false</c>.
      /// </value>
      bool PureBarcode { get; set; }

      /// <summary>
      /// Specifies what character encoding to use when decoding, where applicable (type String)
      /// </summary>
      /// <value>
      /// The character set.
      /// </value>
      string CharacterSet { get; set; }

      /// <summary>
      /// Image is known to be of one of a few possible formats.
      /// Maps to a {@link java.util.List} of {@link BarcodeFormat}s.
      /// </summary>
      /// <value>
      /// The possible formats.
      /// </value>
      IList PossibleFormats { get; set; }

#if !SILVERLIGHT
#if !UNITY
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="barcodeBitmap">The barcode bitmap.</param>
      /// <returns>the result data or null</returns>
      Result Decode(Bitmap barcodeBitmap);
#else
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="barcodeBitmap">The barcode bitmap.</param>
      /// <returns>the result data or null</returns>
      Result Decode(byte[] rawRGB, int width, int height);
#endif
#else
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="barcodeBitmap">The barcode bitmap.</param>
      /// <returns>the result data or null</returns>
      Result Decode(WriteableBitmap barcodeBitmap);
#endif
   }
}
