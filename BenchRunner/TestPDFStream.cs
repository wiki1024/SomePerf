using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Perf.Lib;
using RPackage.Light;

namespace BenchRunner
{
    public class TestPDFStream
    {
        private List<Tuple<string, string>> _formData;
        private string _path = @"C:\FormsAutomation\FormsDevelopmentTemplate\conventional-fha-va-usda.pdf";
        private FileFiller _fileFiller = new FileFiller();
        private FormData _oldFormData;
        private IEnumerable<string> _files;

        public TestPDFStream()
        {
            DirectoryInfo templeteDir = new DirectoryInfo(@"C:\FormsAutomation\FormsDevelopmentTemplate");
            _files = from f in templeteDir.EnumerateFiles()
                     where f.Extension == ".pdf"
                     orderby f.Length ascending
                     select f.FullName;
        }
        [Setup]
        public void SetupData()
        {
            _formData = new List<Tuple<string, string>> { Tuple.Create("FullName", "Hehejjj") };
            _oldFormData = new FormData { Fields = new Dictionary<string, string> { { "FullName", "Hehe" } } };
        }

        [Benchmark]
        public void PoolWrite()
        {
            var stream = PDFFiller.FillSinglePDF(_path, _formData);
            stream.Dispose();
        }

        [Benchmark]
        public void NoPoolWrite()
        {
            var ms = new MemoryStream();
            _fileFiller.FillSinglePDF(_path, ms, _oldFormData, false);
            ms.Dispose();
        }

        [Benchmark]
        public void PoolZip()
        {
            var stream = PDFFiller.FillSinglePDF(_path, _formData);
            using (FileStream fs = File.Create(string.Format(@"D:\dummy\zip\jjj_pool.pdf"),1024*1024))
            {
                stream.CopyTo(fs);
                //PDFFiller.FillManyToZipStream(_files, _formData, fs);
            }
            stream.Dispose();
        }

        [Benchmark]
        public void UnPoolZip()
        {
            var stream =
                _fileFiller.FillSinglePDF(_path, _oldFormData, false);
            using (FileStream fs = File.Create(string.Format(@"D:\dummy\zip\jjj_unpool.pdf"),1024*1024))
            {
                fs.Write(stream,0,stream.Length);
                //_fileFiller.FillManyToZipStream(_files, _oldFormData, fs);
            }
           
        }
    }
}
