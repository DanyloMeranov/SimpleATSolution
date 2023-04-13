using System;
using System.IO;
using System.Threading;

namespace TestTools
{
    public static class FileProcessor
    {
        private static readonly int _threadNumber = -1;
        private static string _fname = "";
        public static bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public static int CreateThreadFlagFile(string name/*, int number*/)
        {
            _fname = name;
            try
            {
                if (ConfigSettingsReader.DebugLvl > 1)
                    File.WriteAllText(_fname, $"Started: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            }
            catch (Exception)
            {
                //ignore
            }
            return _threadNumber;
        }

        public static void AddLine(string line)
        {
            if (ConfigSettingsReader.DebugLvl < 2) return;

            var s = File.ReadAllText(_fname);
            File.WriteAllText(_fname, $"{s}\n{line}");

        }

        public static void DeleteThreadFlagFile()
        {
            if (ConfigSettingsReader.DebugLvl < 2) return;
            try
            {
                AddLine($"Finished: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                if (FileExists(_fname + ".done"))
                {
                    var i = 0;
                    while (FileExists(_fname + ++i + ".done")) { }
                    File.Move(_fname, _fname + i + ".done");
                }
                else
                    File.Move(_fname, _fname + ".done");
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public static void CreateFlag(string fileName)
        {
            if (FileExists(fileName))
                return;
            File.Create(fileName);
        }

        public static void CheckFileExists(string fileName)
        {
            if (FileExists(fileName))
                return;
            throw new Exception($"File '{fileName}' was not found!");
        }

        public static int TryToCreateFile(string fileName, string description, int userCount = 13)
        {
            var userNumber = 1;
            while (true)
            {
                while (FileExists($"{fileName}{userNumber:00}") && userNumber < userCount)
                        userNumber++;
                if (userNumber >= userCount)
                    return -1;
                try
                {
                    File.AppendAllLines($"{fileName}{userNumber:00}", new[] { description });
                    var lines = File.ReadAllLines($"{fileName}{userNumber:00}");
                    if (!lines[0].Equals(description))
                    {
                        userNumber++;
                        continue;
                    }
                }
                catch (Exception)
                {
                    userNumber++;
                    continue;
                }
                return userNumber;
            }
        }

        public static void DeleteFile(string fileName)
        {
            File.Delete(fileName);
            if (!FileExists(fileName))
                return;
            throw new Exception($"File '{fileName}' could not be deleted!");
        }

        public static bool TryToDeleteFile(string fileName)
        {
            var times = 0;
            while (FileExists(fileName) && times++ < 10)
            {
                try
                {
                    Thread.Sleep(100);
                    File.Delete(fileName);
                }
                catch (Exception)
                { }
            }
            return !FileExists(fileName);
        }

        public static string GetTempPath()
        {
            return Path.GetTempPath();
        }
    }
}
