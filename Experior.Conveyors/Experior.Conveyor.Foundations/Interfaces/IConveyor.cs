using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experior.Conveyor.Foundations.Interfaces
{
    public interface IConveyor<T> where T : IStraightPhotoEye, ICurvePhotoEye
    {
    }
}
