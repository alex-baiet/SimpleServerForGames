﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    class Helper {
        public static string ArrayToString<T>(T[] array) {
            string res = "[";
            for (int i = 0; i < array.Length - 1; i++) {
                res += $"{array[i]}, ";
            }
            res += $"{array[array.Length - 1]}]";
            return res;
        }

        public static string ArrayToString(Dictionary<int, string> array) {
            string res = "[";
            foreach (KeyValuePair<int, string> pair in array) {
                res += $"{pair}, ";
            }
            res += $"\b\b]";
            return res;
        }
    }
}