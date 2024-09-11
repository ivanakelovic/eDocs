using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPF_fakture_otpremnice
{
    public static class StringHelper
    {
       
        public static string PretvoriUBezKvacica(string tekst)
        {
           
            var zamјene = new Dictionary<int, string>
    {
        { 0x0110, "D" }, // Đ
        { 0x0111, "d" }, // đ
        { 0x010D, "c" }, // č
        { 0x0107, "c" }, // ć
        { 0x0161, "s" }, // š
        { 0x017E, "z" }, // ž
        { 0x010C, "C" }, // Č
        { 0x0106, "C" }, // Ć
        { 0x0160, "S" }, // Š
        { 0x017D, "Z" }  // Ž
    };

            var sb = new StringBuilder();

            foreach (var c in tekst)
            {
                int unicodeValue = (int)c;

               // MessageBox.Show($"Character: {c}, Unicode: {unicodeValue:X4}");
                if (unicodeValue == 0x00D0)
                {
                    sb.Append("D");
                }
                else if (zamјene.TryGetValue(unicodeValue, out var zamјena))
                {
                    sb.Append(zamјena);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }



        

    }
}
