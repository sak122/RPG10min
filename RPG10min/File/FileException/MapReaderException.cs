using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG10min.File.FileException
{
    class MapReaderException: Exception
    {
        public MapReaderException(string message): base(message)
        {
        }
    }
}
