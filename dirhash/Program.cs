using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using dirhash.Services;
using Autofac;

namespace dirhash
{
    /// <summary>
    /// Основной поток
    /// </summary>
    class Program
    {
        public delegate void FilesStateHandler(int current, int all);
        public static event FilesStateHandler Added;

        private static Queue<KeyValuePair<string, string>> _filesHashQueue = new Queue<KeyValuePair<string, string>>();
        private static Queue<FileInfo> _filesInfoQueue = new Queue<FileInfo>();
        public static int SavedCount = 0;
        public static int AllFilesCount = 0;

        private static ILogService _logService;
        private static IContainer _container;
        private static IFileService _fileService;

        public static ILogService LogService
        {
            get
            {
                if (_logService == null)
                    return _logService = Container.Resolve<ILogService>();
                return _logService;
            }
        }

        public static IFileService FileService
        {
            get
            {
                if (_fileService == null)
                    return _fileService = Container.Resolve<IFileService>();
                return _fileService;
            }
        }

        public static IContainer Container
        {
            get
            {
                if (_container == null)
                    return _container = new IoCConfig().Build();
                return _container;
            }
        }

        /// <summary>
        /// Использует сервис для записи в базу данных лог сообщения
        /// </summary>
        /// <param name="message"></param>
        public static void SaveToLog(string message)
        {
            LogService.Add(message);
        }

        public static void SaveFileEntity(KeyValuePair<string, string> file)
        {
            FileService.Insert(file);
            SavedCount++;
        }
        /// <summary>
        /// Смотрит появились ли записи в хэш очереди и закидывает их в бд
        /// </summary>
        public static void DbWorker()
        {
            while (true)
            {
                if (_filesHashQueue.Count <= 0)
                    continue;

                try
                {
                    KeyValuePair<string, string> file;
                    lock (_filesHashQueue)
                    {
                        file = _filesHashQueue.Dequeue();
                    }
                    SaveFileEntity(file);
                }
                catch (Exception e)
                {
                    SaveToLog(e.Message);
                }
            }
        }

        /// <summary>
        /// Смотрит появились ли записи в очереди файлов на вычисление хэш суммы, вычисляет и ставит в очередь на запись в бд
        /// </summary>
        public static void HashWorker()
        {
            while (true)
            {
                if (_filesInfoQueue.Count <= 0)
                    continue;

                try
                {
                    lock (_filesInfoQueue)
                    {
                        FileInfo file = _filesInfoQueue.Dequeue();

                        string hash = GetFileHash(file.FullName);
                        if (hash != string.Empty)
                        {
                            lock (_filesHashQueue)
                            {
                                _filesHashQueue.Enqueue(new KeyValuePair<string, string>(file.Name, file.FullName));
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    SaveToLog(e.Message);
                }
            }
        }

        public static void StatusVerboser(int current, int all)
        {
            Console.WriteLine($" Обработано: { current } из { all }");
        }

        private static string GetFileHash(string filename)
        {
            string result = "";
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        result = Convert.ToBase64String(md5.ComputeHash(inputStream: stream));
                    }
                }
            }
            catch (Exception e)
            {
                SaveToLog(e.Message);
            }
            return result;
        }

        public static void Main(string[] args)
        {
            DirectoryWalker directoryWalker = new DirectoryWalker();
            directoryWalker.SelectPath();
            Added += StatusVerboser;
            
            //ищем файлы и записываем в очередь для хэширования
            Thread walker = new Thread(() => directoryWalker.Walk(ref _filesInfoQueue, ref AllFilesCount));
            //"слушатель" для вычисления хэшей файлов в очереди
            Thread hashWorker = new Thread(HashWorker);
            //"слушатель" для записи в бд вычисленных хэшей
            Thread dbWorker = new Thread(DbWorker);

            walker.Start();
            hashWorker.Start();
            dbWorker.Start();

            Console.ReadKey();
        }
    }

}