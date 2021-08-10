using System;

#if !SILVERLIGHT
#if !UNITY
using System.Collections;
using System.Drawing;
#endif
#else
using System.Windows.Media.Imaging;
#endif
//using Microsoft.SPOT;

using ZXing.Common;

namespace ZXing
{
   /// <summary>
   /// A smart class to decode the barcode inside a bitmap object
   /// </summary>
   public class BarcodeReader : IBarcodeReader
   {
      public delegate LuminanceSource CreateLuminanceSourceDelegate(Bitmap bmp);
      public delegate Binarizer CreateBinarizerDelegate(LuminanceSource luminanceSource);

#if !SILVERLIGHT
#if !UNITY
      private static readonly CreateLuminanceSourceDelegate defaultCreateLuminanceSource =
         (bitmap) => new RGBLuminanceSource(bitmap, bitmap.Width, bitmap.Height);
#else
      private static readonly Func<byte[], int, int, LuminanceSource> defaultCreateLuminanceSource =
         (rawRGB, width, height) => new RGBLuminanceSource(rawRGB, width, height);
#endif
#else
      private static readonly Func<WriteableBitmap, LuminanceSource> defaultCreateLuminanceSource =
         (bitmap) => new RGBLuminanceSource(bitmap, bitmap.PixelWidth, bitmap.PixelHeight);
#endif
      private static readonly CreateBinarizerDelegate defaultCreateBinarizer =
         (luminanceSource) => new HybridBinarizer(luminanceSource);

      private Reader reader;
      private readonly Hashtable hints;
#if !SILVERLIGHT
#if !UNITY
      private CreateLuminanceSourceDelegate createLuminanceSource;
#else
      private Func<byte[], int, int, LuminanceSource> createLuminanceSource;
#endif
#else
      private readonly Func<WriteableBitmap, LuminanceSource> createLuminanceSource;
#endif
      private readonly CreateBinarizerDelegate createBinarizer;
      private bool usePreviousState;

      /// <summary>
      /// Gets the reader which should be used to find and decode the barcode.
      /// </summary>
      /// <value>
      /// The reader.
      /// </value>
      public Reader Reader
      {
         get
         {
#if ONLY_QRCODE
            return reader ?? (reader = new QrCode.QRCodeReader());
#elif ONLY_AZTEC
            return reader ?? (reader = new Aztec.AztecReader());
#elif ONLY_DATAMATRIX
            return reader ?? (reader = new Datamatrix.DataMatrixReader());
#elif ONLY_MAXICODE
            return reader ?? (reader = new Maxicode.MaxiCodeReader());
#elif ONLY_ONED
            return reader ?? (reader = new OneD.MultiFormatOneDReader(null));
#elif ONLY_PDF417
            return reader ?? (reader = new PDF417.PDF417Reader());
#else
            return reader ?? (reader = new MultiFormatReader());
#endif
         }
      }

      /// <summary>
      /// Gets or sets a method which is called if an important point is found
      /// </summary>
      /// <value>
      /// The result point callback.
      /// </value>
      public event ResultPointHandler ResultPointFound
      {
         add
         {
            if (!hints.Contains(DecodeHintType.NEED_RESULT_POINT_CALLBACK))
            {
               ResultPointCallback callback = resultPoint =>
                                                 {
                                                    if (explicitResultPointFound != null)
                                                       explicitResultPointFound(resultPoint);
                                                 };
               hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK] = callback;
            }
            explicitResultPointFound += value;
            usePreviousState = false;
         }
         remove
         {
            explicitResultPointFound -= value;
            if (explicitResultPointFound == null)
               hints.Remove(DecodeHintType.NEED_RESULT_POINT_CALLBACK);
            usePreviousState = false;
         }
      }

      private event ResultPointHandler explicitResultPointFound;

      /// <summary>
      /// event is executed if a result was found via decode
      /// </summary>
      public event ResultHandler ResultFound;

      /// <summary>
      /// Gets or sets a flag which cause a deeper look into the bitmap
      /// </summary>
      /// <value>
      ///   <c>true</c> if [try harder]; otherwise, <c>false</c>.
      /// </value>
      public bool TryHarder
      {
         get
         {
            if (hints.Contains(DecodeHintType.TRY_HARDER))
               return (bool)hints[DecodeHintType.TRY_HARDER];
            return false;
         }
         set
         {
            if (value)
            {
               hints[DecodeHintType.TRY_HARDER] = true;
               usePreviousState = false;
            }
            else
            {
               if (hints.Contains(DecodeHintType.TRY_HARDER))
               {
                  hints.Remove(DecodeHintType.TRY_HARDER);
                  usePreviousState = false;
               }
            }
         }
      }

      /// <summary>
      /// Image is a pure monochrome image of a barcode. Doesn't matter what it maps to;
      /// use {@link Boolean#TRUE}.
      /// </summary>
      /// <value>
      ///   <c>true</c> if monochrome image of a barcode; otherwise, <c>false</c>.
      /// </value>
      public bool PureBarcode
      {
         get
         {
            if (hints.Contains(DecodeHintType.PURE_BARCODE))
               return (bool)hints[DecodeHintType.PURE_BARCODE];
            return false;
         }
         set
         {
            if (value)
            {
               hints[DecodeHintType.PURE_BARCODE] = true;
               usePreviousState = false;
            }
            else
            {
               if (hints.Contains(DecodeHintType.PURE_BARCODE))
               {
                  hints.Remove(DecodeHintType.PURE_BARCODE);
                  usePreviousState = false;
               }
            }
         }
      }

      /// <summary>
      /// Specifies what character encoding to use when decoding, where applicable (type String)
      /// </summary>
      /// <value>
      /// The character set.
      /// </value>
      public string CharacterSet
      {
         get
         {
            if (hints.Contains(DecodeHintType.CHARACTER_SET))
               return (string)hints[DecodeHintType.CHARACTER_SET];
            return null;
         }
         set
         {
            if (value != null)
            {
               hints[DecodeHintType.CHARACTER_SET] = value;
               usePreviousState = false;
            }
            else
            {
               if (hints.Contains(DecodeHintType.CHARACTER_SET))
               {
                  hints.Remove(DecodeHintType.CHARACTER_SET);
                  usePreviousState = false;
               }
            }
         }
      }

      /// <summary>
      /// Image is known to be of one of a few possible formats.
      /// Maps to a {@link java.util.List} of {@link BarcodeFormat}s.
      /// </summary>
      /// <value>
      /// The possible formats.
      /// </value>
      public IList PossibleFormats
      {
         get
         {
            if (hints.Contains(DecodeHintType.POSSIBLE_FORMATS))
               return (IList)hints[DecodeHintType.POSSIBLE_FORMATS];
            return null;
         }
         set
         {
            if (value != null)
            {
               hints[DecodeHintType.POSSIBLE_FORMATS] = value;
               usePreviousState = false;
            }
            else
            {
               if (hints.Contains(DecodeHintType.POSSIBLE_FORMATS))
               {
                  hints.Remove(DecodeHintType.POSSIBLE_FORMATS);
                  usePreviousState = false;
               }
            }
         }
      }

#if !SILVERLIGHT
#if !UNITY
      /// <summary>
      /// Optional: Gets or sets the function to create a luminance source object for a bitmap.
      /// If null then RGBLuminanceSource is used
      /// </summary>
      /// <value>
      /// The function to create a luminance source object.
      /// </value>
      public CreateLuminanceSourceDelegate CreateLuminanceSource
#else
      /// <summary>
      /// Optional: Gets or sets the function to create a luminance source object for a bitmap.
      /// If null then RGBLuminanceSource is used
      /// </summary>
      /// <value>
      /// The function to create a luminance source object.
      /// </value>
      public Func<byte[], int, int, LuminanceSource> CreateLuminanceSource
#endif
#else
      /// <summary>
      /// Optional: Gets or sets the function to create a luminance source object for a bitmap.
      /// If null then RGBLuminanceSource is used
      /// </summary>
      /// <value>
      /// The function to create a luminance source object.
      /// </value>
      public Func<WriteableBitmap, LuminanceSource> CreateLuminanceSource
#endif
      {
         get
         {
            return createLuminanceSource ?? defaultCreateLuminanceSource;
         }
      }

      /// <summary>
      /// Optional: Gets or sets the function to create a binarizer object for a luminance source.
      /// If null then HybridBinarizer is used
      /// </summary>
      /// <value>
      /// The function to create a binarizer object.
      /// </value>
      public CreateBinarizerDelegate CreateBinarizer
      {
         get
         {
            return createBinarizer ?? defaultCreateBinarizer;
         }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="BarcodeReader"/> class.
      /// </summary>
      public BarcodeReader()
#if ONLY_QRCODE
         : this(new QrCode.QRCodeReader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#elif ONLY_AZTEC
         : this(new Aztec.AztecReader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#elif ONLY_DATAMATRIX
         : this(new Datamatrix.DataMatrixReader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#elif ONLY_MAXICODE
         : this(new Maxicode.MaxiCodeReader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#elif ONLY_ONED
         : this(new OneD.MultiFormatOneDReader(null), defaultCreateLuminanceSource, defaultCreateBinarizer)
#elif ONLY_PDF417
         : this(new PDF417.PDF417Reader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#else
         : this(new MultiFormatReader(), defaultCreateLuminanceSource, defaultCreateBinarizer)
#endif
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="BarcodeReader"/> class.
      /// </summary>
      /// <param name="reader">Sets the reader which should be used to find and decode the barcode.
      /// If null then MultiFormatReader is used</param>
      /// <param name="createLuminanceSource">Sets the function to create a luminance source object for a bitmap.
      /// If null then RGBLuminanceSource is used</param>
      /// <param name="createBinarizer">Sets the function to create a binarizer object for a luminance source.
      /// If null then HybridBinarizer is used</param>
      public BarcodeReader(Reader reader,
#if !SILVERLIGHT
#if !UNITY
         CreateLuminanceSourceDelegate createLuminanceSource,
#else
         Func<byte[], int, int, LuminanceSource> createLuminanceSource,
#endif
#else
         Func<WriteableBitmap, LuminanceSource> createLuminanceSource,
#endif
         CreateBinarizerDelegate createBinarizer
         )
      {
#if ONLY_QRCODE
         this.reader = reader ?? new QrCode.QRCodeReader();
#elif ONLY_AZTEC
         this.reader = reader ?? new Aztec.AztecReader();
#elif ONLY_DATAMATRIX
         this.reader = reader ?? new Datamatrix.DataMatrixReader();
#elif ONLY_MAXICODE
         this.reader = reader ?? new Maxicode.MaxiCodeReader();
#elif ONLY_ONED
         this.reader = reader ?? new OneD.MultiFormatOneDReader(null);
#elif ONLY_PDF417
         this.reader = reader ?? new PDF417.PDF417Reader();
#else
         this.reader = reader ?? new MultiFormatReader();
#endif
         this.createLuminanceSource = createLuminanceSource ?? defaultCreateLuminanceSource;
         this.createBinarizer = createBinarizer ?? defaultCreateBinarizer;
         hints = new Hashtable();
         usePreviousState = false;
      }

#if !SILVERLIGHT
#if !UNITY
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="barcodeBitmap">The barcode bitmap.</param>
      /// <returns>the result data or null</returns>
      public Result Decode(Bitmap barcodeBitmap)
#else
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="rawRGB">raw bytes of the image in RGB order</param>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <returns>
      /// the result data or null
      /// </returns>
      public Result Decode(byte[] rawRGB, int width, int height)
#endif
#else
      /// <summary>
      /// Decodes the specified barcode bitmap.
      /// </summary>
      /// <param name="barcodeBitmap">The barcode bitmap.</param>
      /// <returns>the result data or null</returns>
      public Result Decode(WriteableBitmap barcodeBitmap)
#endif
      {
#if !UNITY
         if (barcodeBitmap == null)
            throw new ArgumentNullException("barcodeBitmap");
#else
         if (rawRGB == null)
            throw new ArgumentNullException("rawRGB");
#endif

         var result = default(Result);
#if !UNITY
         var luminanceSource = CreateLuminanceSource(barcodeBitmap);
#else
         var luminanceSource = CreateLuminanceSource(rawRGB, width, height);
#endif
         var binarizer = CreateBinarizer(luminanceSource);
         var binaryBitmap = new BinaryBitmap(binarizer);
#if !(ONLY_QRCODE || ONLY_AZTEC || ONLY_DATAMATRIX || ONLY_MAXICODE || ONLY_ONED || ONLY_PDF417)
         var multiformatReader = Reader as MultiFormatReader;
#endif
         var rotationCount = 0;
         var rotationMaxCount = 1;

         if (TryHarder)
            rotationMaxCount = 4;

         for (; rotationCount < rotationMaxCount; rotationCount++)
         {
#if !(ONLY_QRCODE || ONLY_AZTEC || ONLY_DATAMATRIX || ONLY_MAXICODE || ONLY_ONED || ONLY_PDF417)

            if (usePreviousState && multiformatReader != null)
            {
               result = multiformatReader.decodeWithState(binaryBitmap);
            }
            else
            {
               result = Reader.decode(binaryBitmap, hints);
               usePreviousState = true;
            }
#else
            result = Reader.decode(binaryBitmap, hints);
#endif

            if (result != null ||
                !luminanceSource.RotateSupported)
               break;
            binaryBitmap = new BinaryBitmap(CreateBinarizer(luminanceSource.rotateCounterClockwise()));
         }

         if (result != null)
         {
            if (result.ResultMetadata == null)
            {
               result.putMetadata(ResultMetadataType.ORIENTATION, rotationCount*90);
            }
            else if (!result.ResultMetadata.Contains(ResultMetadataType.ORIENTATION))
            {
               result.ResultMetadata[ResultMetadataType.ORIENTATION] = rotationCount*90;
            }
            else
            {
               // perhaps the core decoder rotates the image already (can happen if TryHarder is specified)
               result.ResultMetadata[ResultMetadataType.ORIENTATION] = ((int)(result.ResultMetadata[ResultMetadataType.ORIENTATION]) + rotationCount * 90) % 360;
            }

            if (ResultFound != null)
               ResultFound(result);
         }

         return result;
      }
   }
}
