using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Extensions
{
    public static class NumberExtensions
    {
		public static string ToKMB(this double num)
		{
			if (num > 999999999 || num < -999999999)
			{
				return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.#K", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
