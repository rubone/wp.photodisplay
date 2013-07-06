using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Models
{
    public class GenericEventArgs<T> : EventArgs
    {
        public IList<T> Results { get; private set; }

        public GenericEventArgs(IList<T> results)
        {
            Results = results;
        }
    }
}
