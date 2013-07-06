using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVVM.Models
{
    public class ImagesBrand
    {
        public string Name { get; set; }
        public Uri uri { get; set; }
        public BitmapImage Image { get; set; }
    }
}
