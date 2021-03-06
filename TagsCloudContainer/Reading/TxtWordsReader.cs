using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TagsCloudContainer.Reading
{
    public class TxtWordsReader : IWordsReader
    {
        public List<string> ReadWords(string inputPath)
        {
            using (var fileStream = File.OpenRead(inputPath))
            {
                var array = new byte[fileStream.Length];
                fileStream.Read(array, 0, array.Length);
                var textFromFile = System.Text.Encoding.UTF8.GetString(array);
                return textFromFile.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
    }
}