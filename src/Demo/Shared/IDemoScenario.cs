using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Shared
{
    public interface IDemoScenario
    {
        bool Enabled { get; }

        Task RunAsync();
    }
}
