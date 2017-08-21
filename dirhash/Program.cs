using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using dirhash.Services;
using Autofac;
using dirhash.Models;

namespace dirhash
{

    class Program
    {
        #region [properties]
        #region [События]
        /// <summary>
        /// Делегат для вывода текущего статуса выполнения
        /// </summary>
        /// <param name="current"></param>
        /// <param name="hashed"></param>
        /// <param name="all"></param>
        private delegate void FilesStateHandler(int current, int hashed, int all);
        /// <summary>
        /// Событие возникает при добавлении в очереди или запись в бд
        /// </summary>
        private static event FilesStateHandler Added;

        #endregion [События]

        #region [Очереди]

        private static readonly Queue<KeyValuePair<string, string>> FilesHashQueue = new Queue<KeyValuePair<string, string>>();
        private static readonly Queue<FileInfo> FilesInfoQueue = new Queue<FileInfo>();

        #endregion [Очереди]

        #region [Счётчики]

        private static Counters savedCount = new Counters();
        private static Counters hashedCount = new Counters();
        private static Counters allFilesCount = new Counters();

        #endregion [Счётчики]

        #region [DiContainer]
    
        private static Autofac.IContainer Container { get; } = new IoCConfig().Build();

        private static ILogService LogService { get; } = Container.Resolve<ILogService>();

        private static IFileService FileService { get; } = Container.Resolve<IFileService>();
        #endregion [DiContainer]

        #endregion [properties]

        #region [Workers]

        /// <summary>
        /// Смотрит появились ли записи в хэш очереди и записывает их в бд пачками до 30 шт
        /// </summary>
        private static void DbWorker()
        {
            while (true)
            {
                if (FilesHashQueue.Count == 0)
                    Thread.Sleep(1);
                else
                    try
                    {
                        List<KeyValuePair<string, string>> toInsertFiles = new List<KeyValuePair<string, string>>();
                        lock (FilesHashQueue)
                        {
                            //вставляем пачками по 30 шт или сколько там набралось, бережём бд :)
                            for (int i = 0; i <= Math.Min(29, FilesHashQueue.Count); i++)
                            {
                                KeyValuePair<string, string> file = FilesHashQueue.Dequeue();

                                if (file.Value != string.Empty)
                                    toInsertFiles.Add(file);
                            }
                        }
                        if (FileService.Insert(toInsertFiles))
                        {
                            savedCount.Counter+= toInsertFiles.Count;
                            Added?.Invoke(savedCount.Counter, 0, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.Add(e.Message);
                    }
            }
        }

        /// <summary>
        /// Смотрит появились ли записи в очереди файлов на вычисление хэш суммы, вычисляет и ставит в очередь на запись в бд
        /// </summary>
        private static void HashWorker()
        {
            while (true)
            {
                if (FilesInfoQueue.Count == 0)
                    Thread.Sleep(1);
                else
                    try
                    {
                        FileInfo file;
                        lock (FilesInfoQueue)
                        {
                            file = FilesInfoQueue.Dequeue();
                        }

                        string hash = GetFileHash(file.FullName);
                        if (hash != string.Empty)
                        {
                            lock (FilesHashQueue)
                            {
                                FilesHashQueue.Enqueue(new KeyValuePair<string, string>(file.Name, hash));
                            }
                            hashedCount.Counter++;
                            Added?.Invoke(0, hashedCount.Counter, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.Add(e.Message);
                    }
            }

        }

        #region [DirectoryWalker]

        private static string SelectPath()
        {
            bool isExistPath;
            string rootPath;

            do
            {
                Console.Write("Введите путь: ");

                rootPath = Console.ReadLine();
                isExistPath = Directory.Exists(rootPath);

                Console.WriteLine(!isExistPath ? "Путь не найден." : $"Выбрана директория: {rootPath}");

            } while (!isExistPath);

            return rootPath;
        }

        private static int SelectThreadCount()
        {
            int hashThreadsCount = 0;
            do
            {
                Console.WriteLine("Введите количество потоков для вычисления хэша");

                if (!int.TryParse(Console.ReadLine(), out hashThreadsCount)) continue;

            } while (hashThreadsCount <= 0);

            return hashThreadsCount;
        }

        static void Walker(string rootPath)
        {
            Walk(rootPath);
        }

        static void Walk(string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = null;

            try
            {
                files = root.GetFiles("*.*");
            }
            catch (Exception e)
            {
                LogService.Add(e.Message);
            }


            if (files != null)

                foreach (FileInfo file in files)
                {
                    lock (FilesInfoQueue)
                    {
                        FilesInfoQueue.Enqueue(file);
                    }
                    allFilesCount.Counter++;
                    Added?.Invoke(0, 0, allFilesCount.Counter);

                }

            var subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                Walk(dirInfo.FullName);
            }
        }

        #endregion
        #endregion [Workers]

        #region [Heplers]

        private static string GetFileHash(string filename)
        {
            string result = "";
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        result = Convert.ToBase64String(md5.ComputeHash(stream));
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Add(e.Message);
            }
            return result;
        }

        private static void StatusVerboser(int current = 0, int hashed = 0, int all = 0)
        {

            if (current == 0)
                current = savedCount.Counter;

            if (hashed == 0)
                hashed = hashedCount.Counter;

            if (all == 0)
                all = allFilesCount.Counter;

            Console.WriteLine($"hashed: {hashed,3} | all: {all,3}| done: {current,3}");
        }

        #endregion [Heplers]



        public static void Main(string[] args)
        {
            //цепляем событие [при любых действиях выводим сообщение о статусe]
            Added += StatusVerboser;

            string path = SelectPath();
            int threadCount = SelectThreadCount();

            Thread walker = new Thread(() => { Walker(path); });
            walker.Start();

            for (int i = 1; i <= threadCount; i++)
            {
                Thread hasher = new Thread(HashWorker) {Name = "Поток " + i.ToString()};
                hasher.Start();
            }
           
            Thread dbWriter = new Thread(DbWorker);
            dbWriter.Start();
            
            Console.ReadKey();
        }
    }

}