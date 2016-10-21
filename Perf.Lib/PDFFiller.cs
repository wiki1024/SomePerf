using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Perf.Lib.blocks;

namespace Perf.Lib
{
    public class PDFFiller
    {
        public static void FillSinglePDF(string fullPath, Stream output, List<Tuple<string, string>> formData, bool isFlattern = false)
        {
            using (PdfReader reader = new PdfReader(fullPath))
            using (PdfStamper stamper = new PdfStamper(reader, output, false) { FormFlattening = isFlattern })
            {
                AcroFields acroForm = stamper.AcroFields;
                // acroForm.GenerateAppearances = false;
                string actualVal = string.Empty;

                foreach (var pair in formData)
                {
                    acroForm.SetField(pair.Item1, pair.Item2);
                }

                stamper.Close();
            }
        }

        public static void FillManyToZipStream(IEnumerable<string> paths, List<Tuple<string, string>> formData, Stream zipStream, bool isFlattern = false)
        {
            ZipFiles(FillMany(paths, formData, isFlattern), zipStream);
        }

        private static void ZipFiles(List<Tuple<string, PDFStream>> pairs, Stream zipStream)
        {
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var pair in pairs)
                {
                    using (var entryStream = archive.CreateEntry(pair.Item1).Open())
                    {
                        pair.Item2.CopyTo(entryStream);
                        pair.Item2.Dispose();
                    }
                }
            }
        }

        public static List<Tuple<string, PDFStream>> FillMany(IEnumerable<string> paths, List<Tuple<string, string>> formData, bool isFlattern = false)
        {
            paths = paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct();
            var outputBytes = new List<Tuple<string, PDFStream>>();
            foreach (var path in paths)
            {
                var stream = FillSinglePDF(path, formData, isFlattern);
                outputBytes.Add(new Tuple<string, PDFStream>(Path.GetFileName(path),stream));
            }
            return outputBytes;
        }

        public static PDFStream FillSinglePDF(string path, List<Tuple<string, string>> formData, bool isFlattern=false)
        {
            var stream=new PDFStream(MemoryPool.Shared);
            PDFFiller.FillSinglePDF(path, stream, formData);
            return stream;
        }
    }
}
