using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBlog
{
    public static class RandomHelper
    {

        public static string RandomString(this int size, bool lowerCase)
        {
            var builder = new StringBuilder();
            var random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static string RandomId(this int size)
        {
            var builder = new StringBuilder();
            var random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                if (random.Next() % 2 == 0)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }
                else
                {
                    builder.Append(random.Next(0, 9).ToString());
                }
            }

            return builder.ToString();
        }

    }
}
