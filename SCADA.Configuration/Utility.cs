using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SCADA.Configuration
{
    public class Utility
    {
        public static bool TryParse2File(string valueString, out FileInfo fileInfo)
        {
            string valueStringTrimmed = valueString.Trim();

            fileInfo = null;
            string[] validDrives = { "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return false;
            }
            if (!Path.IsPathRooted(valueString))
            {
                return false;
            }
            if (valueString.Length < 3 || !valueString[1].Equals(':') || valueString[2] != '\\')
            {
                return false;
            }
            string drive = valueString[0].ToString().ToUpper();
            if (!validDrives.Any(x => x == drive))
            {
                return false;
            }
            if (valueString.EndsWith("\\"))
            {
                return false;
            }
            valueString = new string(valueString.Skip(3).ToArray());
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return false;
            }
            var subPaths = valueString.Split('\\');
            foreach (var subPath in subPaths)
            {
                if (string.IsNullOrWhiteSpace(subPath))
                {
                    return false;
                }
                if (Path.GetInvalidFileNameChars().Any(c => subPath.Contains(c)))
                {
                    return false;
                }
            }
            fileInfo = new FileInfo(valueStringTrimmed);
            return true;
        }

        public static bool TryParse2Directory(string valueString, out DirectoryInfo directoryInfo)
        {
            string valueStringTrimmed = valueString.Trim();
            directoryInfo = null;
            string[] validDrives = { "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return false;
            }
            if (!Path.IsPathRooted(valueString))
            {
                return false;
            }
            if (valueString.Length < 3 || !valueString[1].Equals(':') || valueString[2] != '\\')
            {
                return false;
            }
            string drive = valueString[0].ToString().ToUpper();
            if (!validDrives.Any(x => x == drive))
            {
                return false;
            }
            valueString = new string(valueString.Skip(3).ToArray());
            if (string.IsNullOrWhiteSpace(valueString))
            {
                directoryInfo = new DirectoryInfo(valueStringTrimmed);
                return true;
            }
            if (valueString.Contains("\\\\"))
            {
                return false;
            }
            valueString = valueString.TrimEnd('\\');
            var subPaths = valueString.Split('\\');

            foreach (var subPath in subPaths)
            {
                if (string.IsNullOrWhiteSpace(subPath))
                {
                    return false;
                }
                if (Path.GetInvalidFileNameChars().Any(c => subPath.Contains(c)))
                {
                    return false;
                }
            }
            directoryInfo = new DirectoryInfo(valueStringTrimmed);
            return true;
        }

        public static bool TryParse2Decimal(string valueString, out decimal @decimal)
        {
            if (long.TryParse(valueString, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out long @long))
            {
                @decimal = @long;
                return true;
            }
            else if (valueString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(valueString.TrimStart('0', 'x', 'X'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out @long))
            {
                @decimal = @long;
                return true;
            }
            else if (decimal.TryParse(valueString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Float, CultureInfo.InvariantCulture, out @decimal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryParse2Long(string valueString, out long @long)
        {
            if (long.TryParse(valueString, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out @long))
            {
                return true;
            }
            else if (valueString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(valueString.TrimStart('0', 'x', 'X'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out @long))
            {
                return true;
            }
            return false;
        }

        public static bool TryParse2DateTime(string valueString, out DateTime dateTime)
        {
            return DateTime.TryParse(valueString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
        }

        public static bool TryParse2Color(string valueString, out System.Drawing.Color color)
        {
            color = System.Drawing.Color.Empty;
            if (!valueString.StartsWith("#"))
            {
                return false;
            }
            valueString = valueString.TrimStart('#');
            if (valueString.Length != 6 && valueString.Length != 8)
            {
                return false;
            }
            if (!Regex.IsMatch(valueString, @"^[0-9a-fA-F]+$"))
            {
                return false;
            }
            color = System.Drawing.ColorTranslator.FromHtml("#" + valueString);
            return true;
        }
    }
}