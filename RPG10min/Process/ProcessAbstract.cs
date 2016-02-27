using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG10min.Process
{
    public abstract class ProcessAbstract
    {
        abstract public void Load();
        abstract public void Main();
        abstract public void Draw();
    }
}
